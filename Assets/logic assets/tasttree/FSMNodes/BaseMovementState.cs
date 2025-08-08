using UnityEngine;
abstract class BasicMovementState : IState
{
    FSMNode fSM;
    public BasicMovementState(FSMNode fSM){
        this.fSM=fSM;
    }
    abstract public void Enter();
    abstract public void Exit();
    abstract public void FixUpdate();
    abstract public void Update();
}
class IdleState : BasicMovementState
{
    IdleState(FSMNode fSM):base(fSM){}
    public override void Enter()
    {
        
    }

    public override void Exit()
    {
       
    }

    public override void FixUpdate()
    {
        
    }

    public override void Update()
    {
        
    }
}