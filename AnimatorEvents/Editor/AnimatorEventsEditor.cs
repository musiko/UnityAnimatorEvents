using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[CustomEditor (typeof(AnimatorEvents))]
public class AnimatorEventsEditor : Editor {
	AnimatorEvents animatorEvents;

	void OnEnable() {
		animatorEvents = target as AnimatorEvents;
		animatorEvents.animator = animatorEvents.gameObject.GetComponent<Animator>();
		if (animatorEvents.CheckRedudancy())
			return;
	}
	
	public override void OnInspectorGUI () {
		if (!Application.isPlaying) {
			if (GUILayout.Button("Update From Animator"))
				animatorEvents.layers = GetAnimatorLayers();
		}
		
		if (animatorEvents.animator == null)
			return;
		
		if (animatorEvents.layers == null)
			return;
		
		string[] layerNames = GetLayerNames(animatorEvents.animator);
	
		for (int i = 0; i < animatorEvents.layers.Length; i++) {
			
			// Draw Layer Foldout
			animatorEvents.layers[i].foldLayer = EditorGUILayout.Foldout(animatorEvents.layers[i].foldLayer, layerNames[i]);
			if (animatorEvents.layers[i].foldLayer) {
				animatorEvents.layers[i].isListening = EditorGUILayout.Toggle("Listen to Events", animatorEvents.layers[i].isListening);
				
				// Draw States Foldout
				animatorEvents.layers[i].foldStates = EditorGUILayout.Foldout(animatorEvents.layers[i].foldStates, "States(" + animatorEvents.layers[i]._stateKeys.Length.ToString() + ")");
				if (animatorEvents.layers[i].foldStates) {
					EditorGUILayout.LabelField("\t" + "Hash Name", "Unique Name");
					for (var j = 0; j < animatorEvents.layers[i]._stateKeys.Length; j++) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("\t" + animatorEvents.layers[i]._stateKeys[j].ToString(), animatorEvents.layers[i]._stateNames[j]);
						EditorGUILayout.EndHorizontal();
					}
				}
				
				//Draw Transition Foldout
				animatorEvents.layers[i].foldTransitions = EditorGUILayout.Foldout(animatorEvents.layers[i].foldTransitions, "Transitions(" + animatorEvents.layers[i]._transitionKeys.Length.ToString() + ")");
				if (animatorEvents.layers[i].foldTransitions) {
					EditorGUILayout.LabelField("\t" + "Hash Name", "Unique Name");
					for (var k = 0; k < animatorEvents.layers[i]._transitionKeys.Length; k++) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("\t" + animatorEvents.layers[i]._transitionKeys[k].ToString(), animatorEvents.layers[i]._transitionNames[k]);
						EditorGUILayout.EndHorizontal();
					}	
				}
			}	
			
		}
	}
	
	public AnimatorEventLayer[] GetAnimatorLayers() {
		List<AnimatorEventLayer> animatorLayers = new List<AnimatorEventLayer>();
		for (int i = 0; i < GetLayerCount(animatorEvents.animator); i++) {
			animatorLayers.Add (new AnimatorEventLayer (
														GetStateKeys(animatorEvents.animator, i),
														GetStateNames(animatorEvents.animator, i),
														GetTransitionKeys(animatorEvents.animator, i),
														GetTransitionNames(animatorEvents.animator, i)));
		}
		return animatorLayers.ToArray();
	}

	#region Animator Layer Methods
	
	/// <summary>
	/// Number of layers.
	/// </summary>
	/// <returns>
	/// The layer count.
	/// </returns>
	/// <param name='animator'>
	/// Animator.
	/// </param>
	private static int GetLayerCount (Animator animator) {
		UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        return animatorController.layers.Length;
	}
	
	/// <summary>
	/// Get all the layer names.
	/// </summary>
	/// <returns>
	/// The layer names.
	/// </returns>
	/// <param name='animator'>
	/// Animator.
	/// </param>
	private static string[] GetLayerNames (Animator animator) {
		UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
		
		List<string> layerNames = new List<string>();
		
		for (int i = 0; i < animatorController.layers.Length; i++)
            layerNames.Add(animatorController.layers[i].name);
		
		return layerNames.ToArray();
	}
	
	#endregion
	
	#region Animator State Methods	
	private static int[] GetStateKeys (Animator animator, int layer) {
		List<int> stateKeys = new List<int>();
		
		UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
		UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[layer].stateMachine;

        stateKeys.AddRange(GetStateKeysFromStateMachine(stateMachine));
        for (int i = 0; i < stateMachine.stateMachines.Length; i++) {
            UnityEditor.Animations.AnimatorStateMachine subStateMachine = stateMachine.stateMachines[i].stateMachine;
            stateKeys.AddRange(GetStateKeysFromStateMachine(subStateMachine));
        }
        		
		return stateKeys.ToArray();
	}

    private static int[] GetStateKeysFromStateMachine (UnityEditor.Animations.AnimatorStateMachine stateMachine) {
        List<int> stateKeys = new List<int>();

        List<UnityEditor.Animations.AnimatorState> states = new List<UnityEditor.Animations.AnimatorState>();
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = stateMachine.states[i].state;
            states.Add(state);
        }

        foreach (UnityEditor.Animations.AnimatorState state in states)
            stateKeys.Add(state.nameHash);

        return stateKeys.ToArray();
    }
	
	private static string[] GetStateNames (Animator animator, int layer) {
		List<string> stateNames = new List<string>();
		
		UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
		UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[layer].stateMachine;

        stateNames.AddRange(GetStateNamesFromStateMachine(stateMachine));
        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            UnityEditor.Animations.AnimatorStateMachine subStateMachine = stateMachine.stateMachines[i].stateMachine;
            stateNames.AddRange(GetStateNamesFromStateMachine(subStateMachine));
        }

        return stateNames.ToArray();
	}

    private static string[] GetStateNamesFromStateMachine (UnityEditor.Animations.AnimatorStateMachine stateMachine) {
        List<string> stateNames = new List<string>();
        
        List<UnityEditor.Animations.AnimatorState> states = new List<UnityEditor.Animations.AnimatorState>();
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = stateMachine.states[i].state;
            states.Add(state);
        }

        foreach (UnityEditor.Animations.AnimatorState state in states)
            stateNames.Add(stateMachine.name + "." + state.name);

        return stateNames.ToArray();
    }
	#endregion
	
	#region Animator Transition Methods
	
	private static int[] GetTransitionKeys (Animator animator, int layer) {
		List<int> transitionKeys = new List<int>();
		
		/*UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[layer].stateMachine;

        transitionKeys.AddRange(GetTransitionKeysFromStateMachine(stateMachine));
        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            UnityEditor.Animations.ChildAnimatorStateMachine subStateMachine = stateMachine.stateMachines[i];
            transitionKeys.AddRange(GetTransitionKeysFromStateMachine(subStateMachine.stateMachine));
        }*/
				
		return transitionKeys.ToArray();
	}

    /*private static int[] GetTransitionKeysFromStateMachine (UnityEditor.Animations.AnimatorStateMachine stateMachine) {
        List<int> transitionKeys = new List<int>();

        List<UnityEditor.Animations.AnimatorStateTransition> transitions = new List<UnityEditor.Animations.AnimatorStateTransition>();
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = stateMachine.states[i].state;
            UnityEditor.Animations.AnimatorStateTransition[] trans = state.transitions;
            transitions.AddRange(trans);
        }

        foreach (UnityEditor.Animations.AnimatorStateTransition transition in transitions)
            transitionKeys.Add(transition.GetInstanceID());

        return transitionKeys.ToArray();
    }*/
	
	private static string[] GetTransitionNames (Animator animator, int layer) {
		List<string> transitionNames = new List<string>();
		
		UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[layer].stateMachine;

        transitionNames.AddRange(GetTransitionNamesFromStateMachine(stateMachine));
        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            UnityEditor.Animations.AnimatorStateMachine subStateMachine = stateMachine.stateMachines[i].stateMachine;
            transitionNames.AddRange(GetTransitionNamesFromStateMachine(subStateMachine));
        }
				
		return transitionNames.ToArray();
	}

    private static string[] GetTransitionNamesFromStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine) {
        List<string> transitionNames = new List<string>();

        List<UnityEditor.Animations.AnimatorStateTransition> transitions = new List<UnityEditor.Animations.AnimatorStateTransition>();
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = stateMachine.states[i].state;
            UnityEditor.Animations.AnimatorStateTransition[] trans = state.transitions;
            transitions.AddRange(trans);

            foreach (UnityEditor.Animations.AnimatorStateTransition transition in trans)
                transitionNames.Add(transition.GetDisplayName(state));
        }

        return transitionNames.ToArray();
    }
	
	/// <summary>
	/// Gets the count of transitions in a layer.
	/// </summary>
	/// <returns>
	/// The transition count.
	/// </returns>
	/// <param name='animator'>
	/// Animator.
	/// </param>
	/// <param name='layer'>
	/// Layer.
	/// </param>
	public static int GetTransitionsCount (Animator animator, int layer) {
		UnityEditor.Animations.AnimatorController animatorController = animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
        UnityEditor.Animations.AnimatorStateMachine stateMachine = animatorController.layers[layer].stateMachine;

        int transitionCount = GetTransitionsCountFromStateMachine(stateMachine);
        for (int i = 0; i < stateMachine.stateMachines.Length; i++)
        {
            UnityEditor.Animations.AnimatorStateMachine subStateMachine = stateMachine.stateMachines[i].stateMachine;
            transitionCount += GetTransitionsCountFromStateMachine(subStateMachine);
        }
		
		return transitionCount;
	}

    public static int GetTransitionsCountFromStateMachine(UnityEditor.Animations.AnimatorStateMachine stateMachine)
    {
        int transitionCount = 0;
        for (int i = 0; i < stateMachine.states.Length; i++)
        {
            UnityEditor.Animations.AnimatorState state = stateMachine.states[i].state;
            transitionCount += state.transitions.Length;
        }

        return transitionCount;
    }
	#endregion
	
	[MenuItem("Component/Miscellaneous/AnimatorEvents")]
    static void AddComponent()
    {
		if (Selection.activeGameObject != null) {
			if (Selection.activeGameObject.GetComponent<AnimatorEvents>() == null)
				Selection.activeGameObject.AddComponent(typeof(AnimatorEvents));
			else
				Debug.LogError("Can have only one AnimatorEvents");
		}
    }
}
