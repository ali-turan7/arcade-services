// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabricMocks;

/// <summary>
///     Simple wrapper for a synchronous IEnumerable of T.
/// </summary>
/// <typeparam name="T"></typeparam>
public class SyncAsyncEnumerable<T> : Microsoft.ServiceFabric.Data.IAsyncEnumerable<T>
{
    private readonly IEnumerable<T> enumerable;

    public SyncAsyncEnumerable(IEnumerable<T> enumerable)
    {
        this.enumerable = enumerable;
    }

    public Microsoft.ServiceFabric.Data.IAsyncEnumerator<T> GetAsyncEnumerator()
    {
        return new SyncAsyncEnumerator<T>(enumerable.GetEnumerator());
    }
}

/// <summary>
///     Simply wrapper for a synchronous IEnumerator of T.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class SyncAsyncEnumerator<T> : Microsoft.ServiceFabric.Data.IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> enumerator;

    public SyncAsyncEnumerator(IEnumerator<T> enumerator)
    {
        this.enumerator = enumerator;
    }


    public T Current => enumerator.Current;

    public void Dispose()
    {
        enumerator.Dispose();
    }

    public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(enumerator.MoveNext());
    }

    public void Reset()
    {
        enumerator.Reset();
    }
}
