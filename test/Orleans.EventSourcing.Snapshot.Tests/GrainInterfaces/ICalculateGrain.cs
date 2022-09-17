using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace Orleans.EventSourcing.Snapshot.Tests;

public interface ICalculateGrain:IGrainWithIntegerKey
{
    Task<int> CalculateAddition(int a, int b);

    Task<int> CalculateSubtraction(int x, int y);

    Task<int> CalculateMultiplication(int i, int j);

    Task<int> CalculateDivision(int o, int p);

    Task<List<int>> GetResultList();
}