using UnityEngine;
using UnityEngine.AI;
using RPG.Core;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction
    {
        private NavMeshAgent m_Agent;
        private Animator m_Animator;
        private ActionScheduler m_ActionScheduler;
        private Ray posRay;
        void Start()
        {
            m_Agent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();
            m_ActionScheduler = GetComponent<ActionScheduler>();
        }

        void Update()
        {
            // Debug.DrawRay(posRay.origin, posRay.direction * 100, Color.red);
            UpdateAnimator();
        }

        /// <summary>
        /// 设置 iaction, 并开始移动
        /// </summary>
        /// <param name="destation"></param>
        public void StartMoveAction(Vector3 destation)
        {
            m_ActionScheduler.StartAction(this);
            MoveTo(destation);
        }

        /// <summary>
        /// 开启agent的移动，然后设定目标位置
        /// </summary>
        /// <param name="destitation"></param>
        public void MoveTo(Vector3 destitation)
        {
            m_Agent.SetDestination(destitation);
            m_Agent.isStopped = false;
        }

        /// <summary>
        /// 取消移动， stop为true则停止
        /// </summary>
        public void Cancel()
        {
            m_Agent.isStopped = true;
        }


        /// <summary>
        /// 更新角色Z轴方向的动画，在三维坐标系下，Z轴一般表示前进
        /// </summary>
        private void UpdateAnimator()
        {
            //得到角色速率（x,y,z三个方向的），此时的速率是相对于世界坐标系
            Vector3 velocity = m_Agent.velocity;

            //将角色速率转换为自身坐标系，因为Animator中的一切设置都是从自身出发，相对于自身的
            Vector3 localVelocity = this.transform.InverseTransformDirection(velocity);

            //我们要的是Z的速率
            float speed = localVelocity.z;

            //Blend Tree 中我们设置了三个动画,idle->walk->run,"Compute Thresholds"中我们选择了 Vecotr z ,
            //即我们用Z的值来作为阈值
            //我们通过“阈值（thresholds）”的改变来改变动画状态
            m_Animator.SetFloat("forward", speed);

        }


    }
}

