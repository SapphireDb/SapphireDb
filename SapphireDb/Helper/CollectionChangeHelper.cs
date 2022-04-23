using System;
using System.Collections.Generic;
using System.Linq;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Internal.Prefilter;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    public static class CollectionChangeHelper
    {
        public static List<ChangeResponse> CalculateRelativeChanges(List<IPrefilterBase> prefilters,
            List<ChangeResponse> allChanges, KeyValuePair<Type, string> property)
        {
            IEnumerable<WherePrefilter> wherePrefilters = prefilters.OfType<WherePrefilter>();

            List<ChangeResponse> completeChanges = allChanges.ToList();
            List<ChangeResponse> oldValuesUnloadResponses = new List<ChangeResponse>();
            List<ChangeResponse> newValuesLoadResponses = new List<ChangeResponse>();

            foreach (WherePrefilter wherePrefilter in wherePrefilters)
            {
                oldValuesUnloadResponses.AddRange(
                    allChanges
                        .Where(change => change.State == ChangeResponse.ChangeState.Modified &&
                                         !wherePrefilter.WhereExpressionCompiled(change.Value) &&
                                         wherePrefilter.WhereExpressionCompiled(change.OriginalValue))
                        .Select(change =>
                        {
                            ChangeResponse newChangeResponse = change.CreateResponse(null, change.Value);
                            newChangeResponse.State = ChangeResponse.ChangeState.Deleted;
                            return newChangeResponse;
                        })
                );

                newValuesLoadResponses.AddRange(
                    allChanges
                        .Where(change => change.State == ChangeResponse.ChangeState.Modified &&
                                         wherePrefilter.WhereExpressionCompiled(change.Value) &&
                                         !wherePrefilter.WhereExpressionCompiled(change.OriginalValue))
                        .Select(change =>
                        {
                            ChangeResponse newChangeResponse = change.CreateResponse(null, change.Value);
                            newChangeResponse.State = ChangeResponse.ChangeState.Added;
                            return newChangeResponse;
                        })
                );
                
                completeChanges = completeChanges
                    .Where((change) =>
                        wherePrefilter.WhereExpressionCompiled(change.Value) &&
                        (change.State != ChangeResponse.ChangeState.Modified ||
                         wherePrefilter.WhereExpressionCompiled(change.OriginalValue)))
                    .ToList();
            }

            IEnumerable<ChangeResponse> changesForWherePrefilter = oldValuesUnloadResponses
                .Concat(newValuesLoadResponses)
                .GroupBy(v => v.Value)
                .Select(g => g.LastOrDefault());

            return completeChanges.Concat(changesForWherePrefilter).ToList();
        }

        public static List<ChangeResponse> CalculateRelativeAuthenticatedChanges(
            ModelAttributesInfo modelAttributesInfo,
            List<ChangeResponse> allChanges, KeyValuePair<Type, string> property, IConnectionInformation connectionInformation,
            IServiceProvider requestServiceProvider)
        {
            if (!modelAttributesInfo.QueryEntryAuthAttributes.Any())
            {
                return allChanges;
            }

            IEnumerable<ChangeResponse> authenticatedChanges = allChanges
                .Where(change => property.Key.CanQueryEntry(connectionInformation, requestServiceProvider,
                                     change.Value) &&
                                 (change.State != ChangeResponse.ChangeState.Modified || property.Key.CanQueryEntry(
                                      connectionInformation, requestServiceProvider,
                                      change.OriginalValue)));


            IEnumerable<ChangeResponse> oldLoadedNotAllowed = allChanges
                .Where(change => change.State == ChangeResponse.ChangeState.Modified &&
                                 !property.Key.CanQueryEntry(connectionInformation, requestServiceProvider,
                                     change.Value) &&
                                 property.Key.CanQueryEntry(connectionInformation, requestServiceProvider,
                                     change.OriginalValue))
                .Select(change =>
                {
                    ChangeResponse newChangeResponse = change.CreateResponse(null, change.Value);
                    newChangeResponse.State = ChangeResponse.ChangeState.Deleted;
                    return newChangeResponse;
                });

            IEnumerable<ChangeResponse> notLoadedNewAllowed = allChanges
                .Where(change => change.State == ChangeResponse.ChangeState.Modified &&
                                 property.Key.CanQueryEntry(connectionInformation, requestServiceProvider,
                                     change.Value) &&
                                 !property.Key.CanQueryEntry(connectionInformation, requestServiceProvider,
                                     change.OriginalValue))
                .Select(change =>
                {
                    ChangeResponse newChangeResponse = change.CreateResponse(null, change.Value);
                    newChangeResponse.State = ChangeResponse.ChangeState.Added;
                    return newChangeResponse;
                });

            return authenticatedChanges.Concat(oldLoadedNotAllowed).Concat(notLoadedNewAllowed).ToList();
        }
    }
}