﻿using System;
using System.Collections.Generic;
using Common;
using Games;
using ReinforcementLearning.Common;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ReinforcementLearning {
    public static class DynamicProgramming {
        public static List<Movement> PolicyIteration(AiAgent agent, GameGrid grid) {
            // init
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            //Generation des etats de départ avec direction random
            var possibleStates = new List<GridState>();
            foreach (var possibleState in possibleGridStates)
            {
                GridState state = new GridState(possibleState);
                int random = Random.Range(0, 4);
                state.BestAction = (Movement)random;
                state.SetArrow();
                possibleStates.Add(state);
            }
            
            float gamma = 0.9f;
            float theta = 0.01f;
            
            PolicyEvaluation(possibleStates, gamma, theta);
            
            GridState nextState = grid.gridState;
            List<Movement> policy = new List<Movement>();
            int maxItterations = 100;
            do
            {
                maxItterations--;
                var tmp = nextState;
                nextState = possibleStates.Find(state => state.Equals(nextState));
                policy.Add(nextState.BestAction);
                nextState = GameManager.Instance.stateDelegate.GetNextState(nextState, nextState.BestAction, possibleStates, out _, out _);
                if (tmp.Equals(nextState)) break;
            } while (nextState != null && maxItterations >= 0);

            return policy;
        }

        private static void PolicyEvaluation(List<GridState> possibleStates, float gamma, float theta)
        {
            
            int maxIterations = 100000;


            float delta = 0;
            do
            {
                delta = 0;
                maxIterations--;
                foreach (GridState gridState in possibleStates)
                {
                    float currentValue = gridState.value; //on récupère la valeur actuelle de l'état
                    float actionValue = 0;
                    //TODO: Maybe nextState est pas null chez moi mais null chez Clément
                    GridState nextState =
                        GameManager.Instance.stateDelegate.GetNextState(gridState, gridState.BestAction, possibleStates, out _,
                            out _);
                    if (nextState != null)
                    {
                        float reward = GameManager.Instance.stateDelegate.GetReward(gridState, gridState.BestAction,
                            possibleStates, out _);
                        float tmpStateValue = reward +
                                              gamma * nextState.value;
                        if (actionValue < tmpStateValue)
                        {
                            actionValue = tmpStateValue;
                        }
                    }

                    gridState.value = actionValue;
                    delta = Mathf.Max(delta, Mathf.Abs(currentValue - actionValue));
                }
            } while (delta > theta && maxIterations >= 0);


            bool stable = true;
            foreach (GridState state in possibleStates)
            {
                if (state != null)
                {
                    Movement currentMovement = state.BestAction;
                    float actionValue = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        Movement move = (Movement)i;

                        GridState nextState =
                            GameManager.Instance.stateDelegate.GetNextState(state, move, possibleStates, out _, out _);

                        if (nextState == state || nextState == null) continue;
                        
                        float reward = GameManager.Instance.stateDelegate.GetReward(state, state.BestAction,
                            possibleStates, out _);
                        float tmpStateValue = reward +
                                              gamma * nextState.value;
                        if (actionValue < tmpStateValue)
                        {
                            actionValue = tmpStateValue;
                            state.BestAction = move;
                        }
                        
                    }

                    if (state.BestAction != currentMovement)
                    {
                        stable = false;
                    }
                }
            }

            if (stable)
            {
                return;
            }

            PolicyEvaluation(possibleStates, gamma, theta);;
        }

        public static List<Movement> ValueIteration(AiAgent agent, GameGrid grid) {
            // init
            var possibleGridStates = agent.GetAllPossibleStates(grid);
            var possibleStates = new List<GridState>();
            foreach (var possibleState in possibleGridStates) {
                GridState state = new GridState(possibleState) {
                    value = 0
                };
                state.SetArrow();
                possibleStates.Add(state);
            }

            float gamma = 0.9f; // facteur de dévaluation
            float delta;
            float theta = 0.01f;

            int maxItterations = 100;
            do {
                maxItterations--;
                delta = 0;
                for (var index = 0; index < possibleStates.Count; ++index) {
                    GridState state = possibleStates[index];
                    float temp = state.value;

                    float max = 0;
                    foreach (Movement actionType in Enum.GetValues(typeof(Movement)))
                    {
                        GameManager gm = GameManager.Instance;
                        StateDelegate stateDelegate = gm.stateDelegate;
                        
                        float reward = stateDelegate.GetReward(state,actionType, possibleStates, out var nextStateTmp);
                        float currentVal = reward + gamma * (nextStateTmp?.value ?? 0);
                        if (max < currentVal) {
                            state.BestAction = actionType;
                            
                            max = currentVal;
                        }
                    }

                    state.value = max;

                    delta = Mathf.Max(delta, Mathf.Abs(temp - state.value));
                }


            } while (delta > theta && maxItterations >= 0);    // until delta < theta
            
            // build end policy
            List<Movement> policy = new List<Movement>();
            var nextState = grid.gridState;
            
            maxItterations = 100;
            do
            {
                maxItterations--;
                var tmp = nextState;
                nextState = possibleStates.Find(state => state.Equals(nextState));
                policy.Add(nextState.BestAction);
                nextState = GameManager.Instance.stateDelegate.GetNextState(nextState, nextState.BestAction, possibleStates, out _, out _);
                if (tmp.Equals(nextState)) break;
            } while (nextState != null && maxItterations >= 0);

            string possibleStatesVals = "Values : ";
            foreach (var state in possibleStates)
            {
                possibleStatesVals += state.value + " ";
            }
            Debug.Log(possibleStatesVals);
            string policyLog = "Policy (" + policy.Count + ") :";
            foreach (var move in policy)
            {
                string dirname = "";
                switch (move)
                {
                    case Movement.Right:
                        dirname = ">";
                        break;
                    case Movement.Down:
                        dirname = "v";
                        break;
                    case Movement.Left:
                        dirname = "<";
                        break;
                    case Movement.Up:
                        dirname = "^";
                        break;
                }
                policyLog += " " + dirname;
            }
            Debug.Log(policyLog);
            return policy;
        }
    }
}