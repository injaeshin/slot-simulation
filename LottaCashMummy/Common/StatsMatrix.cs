using System;
using System.Threading;

public interface IStatsMatrix<T>
{
    T Get(int row, int col);
    void Set(int row, int col, T value);

    void Update(int row, int col, T addValue, Func<T, T, T> updateFunc);

    int RowCount { get; }
    int ColCount { get; }

    IEnumerable<(int row, int col, T value)> GetItems();
    IEnumerable<(int col, T value)> GetRows(int row);
    IEnumerable<(int row, T value)> GetCols(int col);
}

public class StatsMatrix<T> : IStatsMatrix<T>
{
    private readonly T[,] matrix;

    public StatsMatrix(int rowCount, int colCount)
    {
        matrix = new T[rowCount, colCount];
    }

    public int RowCount => matrix.GetLength(0);

    public int ColCount => matrix.GetLength(1);

    public void Clear()
    {
        Array.Clear(matrix, 0, matrix.Length);
    }

    public T Get(int row, int col)
    {
        return matrix[row, col];
    }

    public void Set(int row, int col, T value)
    {
        matrix[row, col] = value;
    }

    public void Update(int row, int col, T addValue, Func<T, T, T> updateFunc)
    {
        // didn't thread safe
        matrix[row, col] = updateFunc(matrix[row, col], addValue);
    }

    public IEnumerable<(int row, int col, T value)> GetItems()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                yield return (row, col, matrix[row, col]);
            }
        }
    }

    public IEnumerable<(int col, T value)> GetRows(int row)
    {
        for (int c = 0; c < ColCount; c++)
        {
            yield return (c, matrix[row, c]);
        }
    }

    public IEnumerable<(int row, T value)> GetCols(int col)
    {
        for (int r = 0; r < RowCount; r++)
        {
            yield return (r, matrix[r, col]);
        }
    }
}

public class ConcurrentStatsMatrixInt : IStatsMatrix<int>
{
    private readonly int[,] matrix;

    public ConcurrentStatsMatrixInt(int rowCount, int colCount)
    {
        matrix = new int[rowCount, colCount];
    }

    public void Clear()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                Interlocked.Exchange(ref matrix[row, col], 0);
            }
        }
    }

    public int RowCount => matrix.GetLength(0);
    public int ColCount => matrix.GetLength(1);

    public int Get(int row, int col)
    {
        return Interlocked.CompareExchange(ref matrix[row, col], 0, 0);
    }

    public void Set(int row, int col, int value)
    {
        Interlocked.Exchange(ref matrix[row, col], value);
    }

    public void Update(int row, int col, int addValue, Func<int, int, int> updateFunc)
    {
        int originalValue, newValue;
        do
        {
            originalValue = matrix[row, col];
            newValue = updateFunc(originalValue, addValue);
        } while (Interlocked.CompareExchange(ref matrix[row, col], newValue, originalValue) != originalValue);
    }

    public IEnumerable<(int row, int col, int value)> GetItems()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                yield return (row, col, matrix[row, col]);
            }
        }
    }

    public IEnumerable<(int col, int value)> GetRows(int row)
    {
        for (int c = 0; c < ColCount; c++)
        {
            yield return (c, matrix[row, c]);
        }
    }

    public IEnumerable<(int row, int value)> GetCols(int col)
    {
        for (int r = 0; r < RowCount; r++)
        {
            yield return (r, matrix[r, col]);
        }
    }

    public void MergeFrom(IStatsMatrix<int> other)
    {
        if (other.RowCount != RowCount || other.ColCount != ColCount)
            throw new ArgumentException("Matrix dimensions must match");

        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                Interlocked.Exchange(ref matrix[row, col], other.Get(row, col));
            }
        }
    }
}

public class ConcurrentStatsMatrixLong : IStatsMatrix<long>
{
    private readonly long[,] matrix;

    public ConcurrentStatsMatrixLong(int rowCount, int colCount)
    {
        matrix = new long[rowCount, colCount];
    }

    public int RowCount => matrix.GetLength(0);
    public int ColCount => matrix.GetLength(1);

    public long Get(int row, int col)
    {
        return Interlocked.Read(ref matrix[row, col]);
    }

    public void Set(int row, int col, long value)
    {
        Interlocked.Exchange(ref matrix[row, col], value);
    }

    public void Update(int row, int col, long addValue, Func<long, long, long> updateFunc)
    {
        long originalValue, newValue;
        do
        {
            originalValue = matrix[row, col];
            newValue = updateFunc(originalValue, addValue);
        } while (Interlocked.CompareExchange(ref matrix[row, col], newValue, originalValue) != originalValue);
    }

    public void Clear()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                Interlocked.Exchange(ref matrix[row, col], 0L);
            }
        }
    }

    public IEnumerable<(int row, int col, long value)> GetItems()
    {
        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                yield return (row, col, matrix[row, col]);
            }
        }
    }

    public IEnumerable<(int col, long value)> GetRows(int row)
    {
        for (int c = 0; c < ColCount; c++)
        {
            yield return (c, matrix[row, c]);
        }
    }

    public IEnumerable<(int row, long value)> GetCols(int col)
    {
        for (int r = 0; r < RowCount; r++)
        {
            yield return (r, matrix[r, col]);
        }
    }

    public void MergeFrom(IStatsMatrix<long> other)
    {
        if (other.RowCount != RowCount || other.ColCount != ColCount)
            throw new ArgumentException("Matrix dimensions must match");

        for (int row = 0; row < RowCount; row++)
        {
            for (int col = 0; col < ColCount; col++)
            {
                Interlocked.Exchange(ref matrix[row, col], other.Get(row, col));
            }
        }
    }
}


// public class ConcurrentStatsMatrix<T> : IStatsMatrix<T>
// {
//     private readonly T[,] matrix;
//     private readonly object[,] _locks;

//     public ConcurrentStatsMatrix(int rowCount, int colCount)
//     {
//         matrix = new T[rowCount, colCount];
//         _locks = new object[rowCount, colCount];

//         for (int row = 0; row < rowCount; row++)
//         {
//             for (int col = 0; col < colCount; col++)
//             {
//                 _locks[row, col] = new object();
//             }
//         }
//     }

//     public int RowCount => matrix.GetLength(0);

//     public int ColCount => matrix.GetLength(1);

//     public T Get(int row, int col)
//     {
//         lock (_locks[row, col])
//         {
//             return matrix[row, col];
//         }
//     }

//     public void Set(int row, int col, T value)
//     {
//         lock (_locks[row, col])
//         {
//             matrix[row, col] = value;
//         }
//     }

//     public void Update(int row, int col, T addValue, Func<T, T, T> updateFunc)
//     {
//         lock (_locks[row, col])
//         {
//             matrix[row, col] = updateFunc(matrix[row, col], addValue);
//         }
//     }

//     public IEnumerable<(int row, int col, T value)> GetItems()
//     {
//         T[,] snapshot = new T[RowCount, ColCount];

//         for (int row = 0; row < RowCount; row++)
//         {
//             for (int col = 0; col < ColCount; col++)
//             {
//                 lock (_locks[row, col])
//                 {
//                     snapshot[row, col] = matrix[row, col];
//                 }
//             }
//         }

//         for (int row = 0; row < RowCount; row++)
//         {
//             for (int col = 0; col < ColCount; col++)
//             {
//                 yield return (row, col, snapshot[row, col]);
//             }
//         }
//     }

//     public IEnumerable<(int col, T value)> GetRows(int row)
//     {
//         T[] rowSnapshot = new T[ColCount];

//         for (int c = 0; c < ColCount; c++)
//         {
//             lock (_locks[row, c])
//             {
//                 rowSnapshot[c] = matrix[row, c];
//             }
//         }

//         for (int c = 0; c < ColCount; c++)
//         {
//             yield return (c, rowSnapshot[c]);
//         }
//     }

//     public IEnumerable<(int row, T value)> GetCols(int col)
//     {
//         T[] colSnapshot = new T[RowCount];

//         for (int r = 0; r < RowCount; r++)
//         {
//             lock (_locks[r, col])
//             {
//                 colSnapshot[r] = matrix[r, col];
//             }
//         }

//         for (int r = 0; r < RowCount; r++)
//         {
//             yield return (r, colSnapshot[r]);
//         }
//     }
// }
