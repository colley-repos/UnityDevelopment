using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public class UnityUseGamepadInSceneView : MonoBehaviour
{
    static UnityUseGamepadInSceneView()
    {
        EditorApplication.update += Update;
    }

    public static Vector3 position;
    public static Quaternion rotation;

    static void Update()
    {
        SceneView view = SceneView.lastActiveSceneView;
        position = view.pivot;
        rotation = view.rotation;

        var gamepad = Gamepad.current;
        if(gamepad == null)
        {
            return;
        }

        Vector2 move = gamepad.leftStick.ReadValue();
        Vector2 rotate = gamepad.rightStick.ReadValue();
        bool up = gamepad.rightShoulder.isPressed;
        bool down = gamepad.leftShoulder.isPressed;

        GameObject _cameraTarget = new GameObject();
        _cameraTarget.transform.position = position;
        _cameraTarget.transform.rotation = rotation;

        _cameraTarget.transform.position += _cameraTarget.transform.forward * move.y;
        _cameraTarget.transform.position += _cameraTarget.transform.right * move.x;
        _cameraTarget.transform.position += up ? _cameraTarget.transform.up * .1f : _cameraTarget.transform.up * 0f;
        _cameraTarget.transform.position -= down ? _cameraTarget.transform.up * .1f : _cameraTarget.transform.up * 0f;


        _cameraTarget.transform.Rotate(Vector3.up, rotate.x * 3f);
        _cameraTarget.transform.Rotate(Vector3.right, -rotate.y * 3f);
        _cameraTarget.transform.rotation = Quaternion.Euler (
            _cameraTarget.transform.eulerAngles.x,
            _cameraTarget.transform.eulerAngles.y,
            0);

        position = _cameraTarget.transform.position;
        rotation = _cameraTarget.transform.rotation;

        view.pivot = position;
        view.rotation = rotation;
        view.size = 0;

        GameObject.DestroyImmediate(_cameraTarget);

    }
}
