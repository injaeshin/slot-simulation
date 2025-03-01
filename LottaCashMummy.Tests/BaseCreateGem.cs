//using System;
//using Xunit;
//using Moq;
//using LottaCashMummy.Game;
//using LottaCashMummy.Buffer;
//using LottaCashMummy.Common;
//using LottaCashMummy.Table;

//namespace LottaCashMummy.Tests
//{
//    public class BaseCreateGemTests
//    {
//        private readonly Mock<IBaseData> mockBaseData;
//        private readonly Mock<IJackpotData> mockJackpotData;
//        private readonly BaseCreateGem baseCreateGem;
//        private readonly Random random;

//        public BaseCreateGemTests()
//        {
//            mockBaseData = new Mock<IBaseData>();
//            mockJackpotData = new Mock<IJackpotData>();
//            baseCreateGem = new BaseCreateGem(mockBaseData.Object, mockJackpotData.Object);
//            random = new Random();
//        }

//        [Fact]
//        public void CreateGem_WithFeatureBonusTrigger_ShouldConvertNormalToGem()
//        {
//            // Arrange
//            var baseStorage = new BaseStorage(new SpinStatistics());
//            var featureTrigger = new FeatureBonusTrigger(FeatureBonusType.Collect, 5, 1);

//            // Act
//            baseCreateGem.CreateGem(baseStorage, random, featureTrigger);

//            // Assert
//            Assert.Equal(5, baseStorage.GemCount);
//        }

//        [Fact]
//        public void CreateGem_WithMummySymbol_ShouldSetRandomGemAttributes()
//        {
//            // Arrange
//            var baseStorage = new BaseStorage(new SpinStatistics());
//            baseStorage.AddSymbol(0, 0, (byte)SymbolType.Mummy);
//            var featureTrigger = new FeatureBonusTrigger(FeatureBonusType.None, 0, 0);

//            // Act
//            baseCreateGem.CreateGem(baseStorage, random, featureTrigger);

//            // Assert
//            Assert.True(baseStorage.HasMummySymbol);
//            //Assert.Equal(baseStorage.GemCount, baseStorage.GemSymbols.Length);
//        }

//        [Fact]
//        public void SetGemValues_ShouldSetCorrectValues()
//        {
//            // Arrange
//            var baseStorage = new BaseStorage(new SpinStatistics());
//            baseStorage.AddSymbol(0, 0, (byte)SymbolType.Gem);
//            baseStorage.AddSymbol(0, 0, (byte)SymbolType.Gem);
//            mockBaseData.Setup(b => b.BaseSymbol.GetRollGemCredit(It.IsAny<Random>())).Returns(100);
//            mockJackpotData.Setup(j => j.Jackpot.TryGetJackpotType(It.IsAny<int>(), out It.Ref<JackpotType>.IsAny)).Returns(true);

//            // Act
//            baseCreateGem.CreateGem(baseStorage, random, new FeatureBonusTrigger(FeatureBonusType.None, 0, 0));

//            // Assert
//            for (int i = 0; i < baseStorage.GemCount; i++)
//            {
//                var value = baseStorage.GetValue((byte)i);
//                Assert.Equal(100, value);
//            }
//        }
//    }
//}
