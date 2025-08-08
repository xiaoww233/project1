using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

class FSMNode : ITreeNode
{
    private TreeRoot root;
    FSMNodeBlackBoard blackBoard;
    TotalBlackBoard totalBlackBoard;
    public void Init(string startstate,List<string> statekeys)
    {
        blackBoard.SetValue("currentState",startstate);
        blackBoard.SetValue("currentStateHash",Animator.StringToHash(startstate));
        blackBoard.SetValue("stateKeys",statekeys);
    }
    
    public void Enter(IBlackBoard blackBoard)
    {
        
    }

    public void Exit()
    {
        throw new System.NotImplementedException();
    }

    public void Update()
    {
        throw new System.NotImplementedException();
    }
}