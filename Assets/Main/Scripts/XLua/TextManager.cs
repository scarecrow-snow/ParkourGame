using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;
using MessagePipe;

using TMPro;
using System;

public class TextManager : MonoBehaviour
{
    [Inject] ISubscriber<TextMessageEventKey, string> _subscriber;

    [SerializeField] TextMessageEventKey _textMessageEventKey;
    IDisposable _disposable;


    void Awake()
    {

        
        var tmp = GetComponent<TextMeshProUGUI>();

        var d = DisposableBag.CreateBuilder();

        _subscriber.Subscribe(_textMessageEventKey, param =>
        {
            tmp.text = param;
            tmp.ForceMeshUpdate();
        }).AddTo(d);

        _disposable = d.Build();
    }



    void OnDestroy()
    {
        _disposable?.Dispose();
    }
}
