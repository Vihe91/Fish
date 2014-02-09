﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Steering;

namespace Steering.States
{
    public class IdleState:State
    {
        static Vector3 initialPos = Vector3.zero;
        public IdleState(GameObject entity):base(entity)
        {
        }

        public override void Enter()
        {
            if (initialPos == Vector3.zero)
            {
                initialPos = entity.transform.position;
            }
            entity.GetComponent<SteeringBehaviours>().path.Waypoints.Add(initialPos);
            entity.GetComponent<SteeringBehaviours>().path.Waypoints.Add(initialPos + new Vector3(-50, 0, -80));
            entity.GetComponent<SteeringBehaviours>().path.Waypoints.Add(initialPos + new Vector3(0, 0, -160));
            entity.GetComponent<SteeringBehaviours>().path.Waypoints.Add(initialPos + new Vector3(50, 0, -80));
            entity.GetComponent<SteeringBehaviours>().path.Looped = true;
            entity.GetComponent<SteeringBehaviours>().turnOffAll();
            entity.GetComponent<SteeringBehaviours>().turnOn(SteeringBehaviours.behaviour_type.follow_path);
            entity.GetComponent<SteeringBehaviours>().turnOn(SteeringBehaviours.behaviour_type.obstacle_avoidance);
        }
        public override void Exit()
        {
            entity.GetComponent<SteeringBehaviours>().path.Waypoints.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            float range = 30.0f;           
            // Can I see the leader?
            GameObject leader = SteeringManager.Instance().currentScenario.leader;
            if ((leader.transform.position - entity.transform.position).magnitude < range)
            {
                // Is the leader inside my FOV
                AIFighter fighter = (AIFighter)Entity;
                fighter.SwicthState(new AttackingState(fighter));
            }
        }
    }
}
