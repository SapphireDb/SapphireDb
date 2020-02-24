using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MergeConflictResolutionModeAttribute : Attribute
    {
        public MergeConflictResolutionMode MergeConflictResolutionConflictResolutionMode { get; set; }
        
        public MergeConflictResolutionModeAttribute(MergeConflictResolutionMode mergeConflictResolutionConflictResolutionMode)
        {
            MergeConflictResolutionConflictResolutionMode = mergeConflictResolutionConflictResolutionMode;
        }
    }
    
    public enum MergeConflictResolutionMode
    {
        Database, Last, ConflictMarkers
    }
}