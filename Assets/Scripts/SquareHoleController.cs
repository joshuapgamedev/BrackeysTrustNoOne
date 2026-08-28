using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareHoleController : MonoBehaviour
{
    [System.Serializable]
    public class InsertedShape
    {
        public string objectId;

        public GameObject visual;

        public Transform startPoint;
        public Transform endPoint;
    }

    [SerializeField] private PlayerController player;
    [SerializeField] private List<InsertedShape> insertedShapes;

    [SerializeField] private float insertDuration = 0.5f;

    public static event System.Action<ObjectController> OnShapeInserted;

    private bool isAnimating = false;

    private void Start()
    {
        foreach (InsertedShape shape in insertedShapes)
        {
            shape.visual.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAnimating)
            return;

        if (!other.CompareTag("ObjectCollider"))
            return;
        ObjectController objectController = other.GetComponentInParent<ObjectController>();

        if (objectController == null)
            return;

        // Only steal it if the player is actually holding it
        if (!player.IsHolding(objectController))
            return;

        InsertedShape insertedShape = insertedShapes.Find(shape => shape.objectId == objectController.objectId);

        if (insertedShape == null)
        {
            Debug.Log($"inserted shape missing for {objectController.objectId}");
            return;
        }

        StartCoroutine(InsertShape(objectController, insertedShape));
    }

    private IEnumerator InsertShape(ObjectController original, InsertedShape insertedShape)
    {
        isAnimating = true;

        player.ReleaseHeldObject(false);

        original.gameObject.SetActive(false);

        insertedShape.visual.SetActive(true);

        Transform shape = insertedShape.visual.transform;

        shape.position = insertedShape.startPoint.position;
        shape.rotation = insertedShape.startPoint.rotation;

        Vector3 startPos = insertedShape.startPoint.position;
        Vector3 endPos = insertedShape.endPoint.position;

        Quaternion startRot = insertedShape.startPoint.rotation;
        Quaternion endRot = insertedShape.endPoint.rotation;

        float timer = 0f;

        while (timer < insertDuration)
        {
            timer += Time.deltaTime;

            float t = timer / insertDuration;

            shape.position = Vector3.Lerp(startPos, endPos, t);

            shape.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        shape.position = endPos;
        shape.rotation = endRot;

        OnShapeInserted?.Invoke(original);

        isAnimating = false;
    }
}