using UnityEngine;
using UnityEngine.SceneManagement;

public class InstructionController : MonoBehaviour
{
    // Called from animation event at the end
    public void OnInstructionFinished()
    {
        SceneManager.LoadScene("NewMap");
    }
}
