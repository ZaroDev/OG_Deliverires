using UnityEngine;
using UnityEngine.SceneManagement;

namespace Exercise2.UI
{
    public class BackButton : MonoBehaviour
    {
        public void Back()
        {
            SceneManager.LoadScene("Scenes/Exercise2/MainMenu");
        }
    }
}
