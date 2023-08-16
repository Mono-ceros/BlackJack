using UnityEngine;

public class DebugDrag : MonoBehaviour
{
    public Camera cam;

    GameObject obj;

    Vector3 offset;

    int previousZOrder;

	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                obj = hit.collider.gameObject;

                offset = hit.transform.position - hit.point;

                var sprite = hit.collider.GetComponent<SpriteRenderer>();
                previousZOrder = sprite.sortingOrder;

                sprite.sortingOrder = 100;
            }
        }		
        else if (Input.GetMouseButtonUp(0) && obj != null)
        {
            var sprite = obj.GetComponent<SpriteRenderer>();
            sprite.sortingOrder = previousZOrder;

            obj = null;
        }

        if (obj != null)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            mousePos.z = obj.transform.position.z;

            obj.transform.position = mousePos + offset;
        }
	}
}
