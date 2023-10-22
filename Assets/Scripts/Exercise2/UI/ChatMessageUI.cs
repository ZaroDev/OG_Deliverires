using Exercise2.Chat;
using TMPro;
using UnityEngine;

namespace Exercise2.UI
{
    public class ChatMessageUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI userText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI dateText;

        public void SetMessages(Message message)
        {
            userText.text = message.userName;
            messageText.text = message.message;
            dateText.text = message.date;
        }
    }
}