using UnityEngine;

namespace Monster.Components
{
    public class EntityComponent : MonoBehaviour
    {
        /// The GameObject of this entity
        protected GameObject entity;
        /// The Rigidbody of this entity
        protected Rigidbody rb;
        /// The global position of this entity
        protected Vector3 position => transform.position;

        // Set field values
        protected void Awake()
        {
            entity = gameObject;
            rb = entity.GetComponent<Rigidbody>();
        }
    }
}
