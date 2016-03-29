using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

  namespace GameEngine.Ai
  {
  
  public class State<T>
    {
        internal readonly Action OnEnter;
        internal readonly Action OnExit;
        internal readonly List<Transition<T>> Transitions = new List<Transition<T>>();

        public State(string name, T tag, Action onEnter, Action onExit)
        {
            OnEnter = onEnter;
            OnExit = onExit;
            Name = name;
            Tag = tag;
        }

        public string Name { get; private set; }
        public T Tag { get; set; }

        public void AddTransition(State<T> nextState, Func<bool> condition)
        {
            Transitions.Add(new Transition<T>(nextState, condition));
        }
    }

    internal class Transition<T>
    {
        internal readonly Func<bool> Condition;
        internal readonly State<T> NextState;


        internal Transition(State<T> nextState, Func<bool> condition)
        {
            NextState = nextState;
            Condition = condition;
        }
    }

    public class StateMachine<T>
    {
        public StateMachine(State<T> currentState)
        {
            CurrentState = currentState;
        }

        public State<T> CurrentState { get; private set; }

        public void Update()
        {
            while (MoveToNext()) { }
        }

        private bool MoveToNext()
        {
            for (int i = 0; i < CurrentState.Transitions.Count; i++)
            {
                Transition<T> t = CurrentState.Transitions[i];
                if (t.Condition())
                {
                    if (CurrentState.OnExit != null) CurrentState.OnExit();
                    CurrentState = t.NextState;
                    if (CurrentState.OnEnter != null) CurrentState.OnEnter();
                    return true;
                }
            }
            return false;
        }
    }
    
    }