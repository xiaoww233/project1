using UnityEngine;

enum Staus{
    UnStart,
    Running,
    Success,
    Fail
}

interface ITreeNode{
    void Enter(IBlackBoard blackBoard=default);
    void Update();
    void Exit();
}

interface IGaurd{
    Staus StausCheck();
}


