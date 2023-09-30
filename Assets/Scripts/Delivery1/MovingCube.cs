using UnityEngine;

namespace Delivery1
{
    public class MovingCube : MonoBehaviour
    {
        public float speed = 5;
        private Vector3 _startingPos;
        public Vector3 endPos;
        private Vector3 _targetPos;
        void Start()
        {
            _startingPos = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 currentPos = transform.position;
 
            if(currentPos == _startingPos) {
                _targetPos = endPos;
            }
            else if (currentPos == endPos) {
                _targetPos = _startingPos;
            }
            transform.position = Vector3.MoveTowards(currentPos, _targetPos, speed * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_targetPos, 0.5f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPos, 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(_startingPos, 0.5f);
        }
    }
}
