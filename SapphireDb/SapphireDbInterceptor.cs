using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SapphireDb.Command.Subscribe;

namespace SapphireDb;

public class SapphireDbInterceptor : IDbTransactionInterceptor, ISaveChangesInterceptor
{
    private readonly ISapphireDatabaseNotifier _notifier;

    private bool _transactionActive = false;
    
    private readonly List<ChangeResponse> _completeTransactionChanges = new List<ChangeResponse>();
    private readonly List<ChangeResponse> _currentSaveChanges = new List<ChangeResponse>();

    public SapphireDbInterceptor(ISapphireDatabaseNotifier notifier)
    {
        _notifier = notifier;
    }

    public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        _currentSaveChanges.AddRange(GetChanges(eventData));
        return result;
    }

    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _currentSaveChanges.AddRange(GetChanges(eventData));
        return result;
    }
    
    public int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_transactionActive)
        {
            _completeTransactionChanges.AddRange(_currentSaveChanges);
        }
        else
        {
            _notifier.HandleChanges(_currentSaveChanges.ToList(), eventData.Context?.GetType());
        }
        
        _currentSaveChanges.Clear();

        return result;
    }

    public async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (_transactionActive)
        {
            _completeTransactionChanges.AddRange(_currentSaveChanges);
        }
        else
        {
            _notifier.HandleChanges(_currentSaveChanges.ToList(), eventData.Context?.GetType());
        }
        
        _currentSaveChanges.Clear();
        return result;
    }
    
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _currentSaveChanges.Clear();
    }

    public async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _currentSaveChanges.Clear();
    }
    
    public DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        _transactionActive = true;
        return result;
    }
    
    public async ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _transactionActive = true;
        return result;
    }
    
    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        _transactionActive = false;
        _notifier.HandleChanges(_completeTransactionChanges.ToList(), eventData.Context?.GetType());
        _completeTransactionChanges.Clear();
    }
    
    public async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _transactionActive = false;
        _notifier.HandleChanges(_completeTransactionChanges.ToList(), eventData.Context?.GetType());
        _completeTransactionChanges.Clear();
    }

    public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        _transactionActive = false;
        _completeTransactionChanges.Clear();
    }
    
    public async Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _transactionActive = false;
        _completeTransactionChanges.Clear();
    }
    
    private List<ChangeResponse> GetChanges(DbContextEventData eventData)
    {
        return eventData?.Context?.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Deleted || e.State == EntityState.Modified)
            .Select(e => new ChangeResponse(e))
            .Where(c => c.ValidChange)
            .ToList();
    }

    // NOOP interface implementations
    public InterceptionResult<DbTransaction> TransactionStarting(DbConnection connection, TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result)
    {
        return result;
    }

    public async ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(DbConnection connection, TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result, CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result)
    {
        return result;
    }

    public async ValueTask<DbTransaction> TransactionUsedAsync(DbConnection connection, TransactionEventData eventData, DbTransaction result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }
    
    public async ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public InterceptionResult TransactionRollingBack(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public async ValueTask<InterceptionResult> TransactionRollingBackAsync(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public InterceptionResult CreatingSavepoint(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    public async ValueTask<InterceptionResult> CreatingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public async Task CreatedSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
    }

    public InterceptionResult RollingBackToSavepoint(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    public async ValueTask<InterceptionResult> RollingBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public async Task RolledBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
    }

    public InterceptionResult ReleasingSavepoint(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    public async ValueTask<InterceptionResult> ReleasingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return result;
    }

    public async Task ReleasedSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
    }

    public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
    }

    public async Task TransactionFailedAsync(DbTransaction transaction, TransactionErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
    }
}