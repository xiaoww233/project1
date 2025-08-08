using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

class TreeRoot : ITreeNode
{
    
    TotalBlackBoard totalBlackBoard;
    TreeRoot(){
        totalBlackBoard=new TotalBlackBoard();
    }
    public void Enter(IBlackBoard blackBoard = null)
    {
        throw new System.NotImplementedException();
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
