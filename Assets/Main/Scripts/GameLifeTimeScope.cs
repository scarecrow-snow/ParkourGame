using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VContainer;
using VContainer.Unity;
using MessagePipe;

public class GameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // MessagePipeの設定
        var options = builder.RegisterMessagePipe();


        builder.RegisterMessageBroker<TextMessageEventKey, string>(options);
        
    }
}
