using SpinOfFortune.Statistics;

namespace SpinOfFortune.ThreadBuffer;

public class ThreadStorage
{
    public BaseStorage BaseStorage { get; set; }
    public BonusStorage BonusStorage { get; set; }
    private readonly Random random = new Random();

    public ThreadStorage()
    {
        BaseStorage = new BaseStorage(random);
        BonusStorage = new BonusStorage();
    }
}

