
using LottaCashMummy.Common;
using System.Diagnostics;


namespace LottaCashMummy.Tests
{
    public class FeatureSymbolValueTest
    {
        private readonly IFeatureData fd;
        private readonly Random random;
        private const int TOTAL_ITERATIONS = 1_000_000;

        public FeatureSymbolValueTest(IFeatureData featureData)
        {
            fd = featureData;
            random = new Random(42); // 고정된 시드 값으로 재현 가능한 결과 생성
        }

        public void RunTest()
        {
            Console.WriteLine("===== 젬 심볼 값 분포 테스트 시작 =====\n");

            // 모든 레벨에 대해 테스트 (1-4)
            for (int level = 1; level <= 1; level++)
            {
                // 다양한 보너스 타입에 대해 테스트
                TestSymbolValueDistribution(level, FeatureBonusType.Collect);
                // TestSymbolValueDistribution(level, FeatureBonusType.Spins);
                // TestSymbolValueDistribution(level, FeatureBonusType.Symbols);
                // TestSymbolValueDistribution(level, FeatureBonusType.CollectSpins);
                // TestSymbolValueDistribution(level, FeatureBonusType.CollectSymbols);
                // TestSymbolValueDistribution(level, FeatureBonusType.SpinsSymbols);
                // TestSymbolValueDistribution(level, FeatureBonusType.CollectSpinsSymbols);
            }

            Console.WriteLine("===== 젬 심볼 값 분포 테스트 완료 =====");
        }

        private void TestSymbolValueDistribution(int level, FeatureBonusType bonusType)
        {
            Console.WriteLine($"\n레벨: {level}, 보너스 타입: {bonusType}");

            var sw = new Stopwatch();
            sw.Start();

            // 값 분포를 저장할 딕셔너리
            var valueDistribution = new Dictionary<double, int>();
            
            // 총 값을 누적할 변수
            double totalValue = 0;

            // 지정된 횟수만큼 반복
            for (int i = 0; i < TOTAL_ITERATIONS; i++)
            {


                // 심볼 값 가져오기
                var symbolValue = fd.Symbol.GetRollSymbolValues(level, bonusType, random);
                
                // 값 분포 업데이트
                if (!valueDistribution.ContainsKey(symbolValue.Value))
                {
                    valueDistribution[symbolValue.Value] = 0;
                }
                valueDistribution[symbolValue.Value]++;
                
                // 총 값 누적
                totalValue += symbolValue.Value;
            }

            sw.Stop();
            
            // 평균 값 계산
            double averageValue = totalValue / TOTAL_ITERATIONS;
            
            Console.WriteLine($"테스트 실행 시간: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"평균 값: {averageValue:F4}\n");
            
            // 값 분포 출력
            Console.WriteLine("값 분포:");
            foreach (var kvp in valueDistribution.OrderBy(x => x.Key))
            {
                double percentage = (double)kvp.Value / TOTAL_ITERATIONS * 100;
                Console.WriteLine($"  값: {kvp.Key:F2}, 빈도: {kvp.Value}, 비율: {percentage:F2}%");
            }
        }
    }
} 