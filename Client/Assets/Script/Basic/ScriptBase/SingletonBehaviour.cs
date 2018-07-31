// **********************************************************************
// Copyright (c) 2010 All Rights Reserved
// File     : SingletonBehaviour.cs
// Author   : 
// Created  : 
// Porpuse  : 
// **********************************************************************
using UnityEngine;
using System;
using System.Collections.Generic;
/***********************************************************************/
public class SingletonBehaviour<S> : MonoBehaviour where S : MonoBehaviour
{
        private static S mSingleton;

        public virtual void Awake()
        {
                RestSingleton();
                //mSingleton = (S)(MonoBehaviour)this;
                //Debuger.LogError("SingletonBehaviour<S> Awake = " + mSingleton.GetType().ToString());
        }

        public static S GetSingleton()
        {
                return mSingleton;
        }

        public static S GetInst()
        {
                return GetSingleton();
        }

        public static S GetInstance()
        {
                return GetSingleton();
        }


        public static S Instance
        {
                get
                {
                        return GetSingleton();
                }
        }

        protected void RestSingleton()
        {
                mSingleton = (S)(MonoBehaviour)this;
        }

        //void OnDisable()
        //{
        //        mSingleton = (S)GameObject.FindObjectOfType(typeof(S));
        //}

}
