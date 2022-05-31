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

    public ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _currentSaveChanges.AddRange(GetChanges(eventData));
        return ValueTask.FromResult(result);
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

    public ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
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
        return ValueTask.FromResult(result);
    }
    
    public void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _currentSaveChanges.Clear();
    }

    public Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _currentSaveChanges.Clear();
        return Task.CompletedTask;
    }
    
    public DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        _transactionActive = true;
        return result;
    }
    
    public ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _transactionActive = true;
        return ValueTask.FromResult(result);
    }
    
    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        _transactionActive = false;
        _notifier.HandleChanges(_completeTransactionChanges.ToList(), eventData.Context?.GetType());
        _completeTransactionChanges.Clear();
    }
    
    public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _transactionActive = false;
        _notifier.HandleChanges(_completeTransactionChanges.ToList(), eventData.Context?.GetType());
        _completeTransactionChanges.Clear();
        return Task.CompletedTask;
    }

    public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        _transactionActive = false;
        _completeTransactionChanges.Clear();
    }
    
    public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        _transactionActive = false;
        _completeTransactionChanges.Clear();
        return Task.CompletedTask;
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

    public ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(DbConnection connection, TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result, CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result)
    {
        return result;
    }

    public ValueTask<DbTransaction> TransactionUsedAsync(DbConnection connection, TransactionEventData eventData, DbTransaction result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }
    
    public ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public InterceptionResult TransactionRollingBack(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public ValueTask<InterceptionResult> TransactionRollingBackAsync(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public InterceptionResult CreatingSavepoint(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    public ValueTask<InterceptionResult> CreatingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public Task CreatedSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public InterceptionResult RollingBackToSavepoint(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    public ValueTask<InterceptionResult> RollingBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result, CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public Task RolledBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public InterceptionResult ReleasingSavepoint(DbTransaction transaction, TransactionEventData eventData,
        InterceptionResult result)
    {
        return result;
    }

    public void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
    {
    }

    public ValueTask<InterceptionResult> ReleasingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return ValueTask.FromResult(result);
    }

    public Task ReleasedSavepointAsync(DbTransaction transaction, TransactionEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }

    public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
    {
    }

    public Task TransactionFailedAsync(DbTransaction transaction, TransactionErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.CompletedTask;
    }
}