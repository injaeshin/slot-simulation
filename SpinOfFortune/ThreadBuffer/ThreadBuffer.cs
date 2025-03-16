namespace SpinOfFortune.ThreadBuffer;


public class ThreadBuffer
{
    public BaseStorage BaseStorage { get; set; }
    public BonusStorage BonusStorage { get; set; }
    public ThreadBufferResult Result { get; set; }

    public ThreadBuffer()
    {
        BaseStorage = new BaseStorage();
        BonusStorage = new BonusStorage();
        Result = new ThreadBufferResult();
    }
}

