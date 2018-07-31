using System;
using System.Collections.Generic;

public class SingletonObject<S> where S : class , new ()
{
    private static S mSingleton;

    protected SingletonObject()
    {
    }

    public static S GetInstance()
    {
        if (mSingleton == null)
        {
            mSingleton = (S)Activator.CreateInstance(typeof(S));
        }
        return mSingleton;
    }

    public static S Instance()
    {
        return GetInstance();
    }

    public static S GetInst()
    {
        return GetInstance();
    }
}