using System;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace GoodCat.Fsm
{
    public class FsmDebugView : MonoBehaviour
    {
        public bool isPauseOnStateChange;

        [SerializeField] [UsedImplicitly] 
        private string stateCurrentString;

        [Multiline] [SerializeField] [UsedImplicitly]
        private string transition;

        private string _statePreviousName;

        private FSM _fsm;

        private void Start()
        {
            _fsm = GetComponent<IFsmContainer>().Fsm;
            Assert.IsNotNull(_fsm);
            _statePreviousName = "";
            stateCurrentString = _fsm.StateCurrent.ToString();
            transition = "";
            _fsm.OnStateChange += OnTransition;
        }

        private void OnDestroy()
        {
            _fsm.OnStateChange -= OnTransition;
        }

        private void OnTransition(State statePrevious, State stateCurrent)
        {
            _statePreviousName = statePrevious?.ToString();
            stateCurrentString = stateCurrent.ToString();
#if UNITY_EDITOR
            if (isPauseOnStateChange) EditorApplication.isPaused = true;
#endif
            transition = $"{_statePreviousName} => {stateCurrent}";
        }
    }
}