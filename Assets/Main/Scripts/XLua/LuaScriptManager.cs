/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using XLua;
using System;


using VContainer;
using MessagePipe;


using Cysharp.Threading.Tasks;

[System.Serializable]
public class Injection
{
    public string name;
    public GameObject value;
}


public class LuaScriptManager : MonoBehaviour
{
    private static LuaScriptManager _instance;

    [Inject] private readonly IPublisher<TextMessageEventKey, string> _publisher;
    [SerializeField] TextMessageEventKey _textMessageEventKey;

    [SerializeField] Button _button;
    public TextAsset luaScript;
    public Injection[] injections;

    internal static LuaEnv luaEnv = new LuaEnv(); //all lua behaviour shared one luaenv only!
    internal static float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 

    private Action luaStart;
    private Action luaUpdate;
    private Action luaOnDestroy;

    private LuaTable scriptEnv;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        scriptEnv = luaEnv.NewTable();


        LuaTable meta = luaEnv.NewTable();
        meta.Set("__index", luaEnv.Global);
        scriptEnv.SetMetaTable(meta);
        meta.Dispose();

        scriptEnv.Set("self", this);
        foreach (var injection in injections)
        {
            scriptEnv.Set(injection.name, injection.value);
        }

        // Coroutine対応
        Coroutine InvokeStartCoroutine(IEnumerator routine) => StartCoroutine(routine);
        void InvokeStopCoroutine(Coroutine coroutine) => StopCoroutine(coroutine);
        luaEnv.Global.Set("csStartCoroutine", (Func<IEnumerator, Coroutine>)InvokeStartCoroutine);
        luaEnv.Global.Set("csStopCoroutine", (Action<Coroutine>)InvokeStopCoroutine);

        // 起点となるLuaスクリプトを呼び出す
        luaEnv.DoString(luaScript.text, "LuaTestScript", scriptEnv);

        // Monobehavior の 各種イベントに対応したメソッドを 設定する
        scriptEnv.Get("start", out luaStart);
        scriptEnv.Get("update", out luaUpdate);
        scriptEnv.Get("ondestroy", out luaOnDestroy);

        if (luaStart != null)
        {
            luaStart();
        }
    }

    void Update()
    {
        if (luaUpdate != null)
        {
            luaUpdate();
        }
        if (Time.time - LuaScriptManager.lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            LuaScriptManager.lastGCTime = Time.time;
        }
    }

    void OnDestroy()
    {
        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaOnDestroy = null;

        luaStart = null;
        luaUpdate = null;

        scriptEnv.Dispose();
        injections = null;
    }

//----------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>

    [LuaCallCSharp]
    public static void SetText(string text)
    {
        _instance._publisher.Publish(_instance._textMessageEventKey, text);
    }


    /// <summary>
    /// キー入力待機処理
    /// 自動送りをつけること
    /// TODO 現在の状況に応じて待機システムを変更する
    /// </summary>
    /// <returns></returns>
    [LuaCallCSharp]
    public static IEnumerator WaitClicked()
    {
        yield return _instance._button.OnClickAsync().ToCoroutine();
    }
}

