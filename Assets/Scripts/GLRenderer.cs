using UnityEngine;
using System.Collections.Generic;

public interface IGL
{
    void Draw();
}

public class GLRenderer : MonoBehaviour
{
    private static GLRenderer m_instance;

	public static GLRenderer Instance
	{
		get { return m_instance; }
	}

    private List<IGL> m_renderObjects = new List<IGL>();

    public void Add(IGL gl)
    {
        if (m_renderObjects.Contains(gl)) return;
        m_renderObjects.Add(gl);
    }
    public void Remove(IGL gl)
    {
        m_renderObjects.Remove(gl);
    }

    private void Awake()
	{
		if(m_instance != null)
		{
			Debug.LogWarning("Another instance of GLLinesRenderer aleready exist");
		}
		m_instance = this;
	  
	}

	private void OnDestroy()
	{
		if(m_instance == this)
		{
			m_instance = null;
		}
	}
    
    void OnPostRender()
    {
        for (int i = 0; i < m_renderObjects.Count; i++)
        {
            IGL gl = m_renderObjects[i];
            gl.Draw();
        }
    }

}

