﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuComponent : MonoBehaviour 
{
	public MenuComponentType menuComponentType;

	[Header ("Above Menu")]
	public MenuComponent aboveMenuScript;

	[Header ("Other Menu")]
	public List<RectTransform> otherMenuList;
	public List<RectTransform> otherButtonsList;

	[Header ("Under Menu")]
	public List<RectTransform> underMenuList;
	public List<RectTransform> underButtonsList;

	[Header ("Button")]
	public RectTransform button;

	[Header ("Content")]
	public RectTransform content = null;

	void Awake ()
	{
		if (menuComponentType == MenuComponentType.MainMenu)
			MainMenuSetup ();
	}

	void MainMenuSetup ()
	{
		for(int i = 0; i < transform.childCount; i++)
			underMenuList.Add (transform.GetChild (i).GetComponent<RectTransform> ());

		for (int i = 0; i < underMenuList.Count; i++)
			underButtonsList.Add (underMenuList [i].transform.GetChild (0).GetComponent<RectTransform> ());
		
		//Setup Buttons Child Index
		for (int i = 0; i < underButtonsList.Count; i++)
			underButtonsList [i].GetComponent<MenuButtonComponent> ().buttonIndex = i;

		//Call Setup In Under Menu
		if(underMenuList.Count > 0)
			for (int i = 0; i < underMenuList.Count; i++)
				underMenuList [i].GetComponent<MenuComponent> ().OtherMenuSetup ();

		HideAll ();

		DisableAll ();
	}

	public void OtherMenuSetup ()
	{
		if (transform.childCount > 2)
			menuComponentType = MenuComponentType.ButtonsListMenu;
		else
			menuComponentType = MenuComponentType.ContentMenu;

		//Get Menu Button
		button = transform.GetChild (0).GetComponent<RectTransform> ();

		//Get Above Menu
		aboveMenuScript = transform.parent.GetComponent<MenuComponent> ();

		otherMenuList = aboveMenuScript.underMenuList;
		otherButtonsList = aboveMenuScript.underButtonsList;

		//Get Buttons List or Content
		if(menuComponentType == MenuComponentType.ButtonsListMenu)
		{
			for(int i = 1; i < transform.childCount; i++)
				underMenuList.Add (transform.GetChild (i).GetComponent<RectTransform> ());

			for (int i = 0; i < underMenuList.Count; i++)
				underButtonsList.Add (underMenuList [i].transform.GetChild (0).GetComponent<RectTransform> ());
			
			//Setup Buttons Child Index
			for (int i = 0; i < underButtonsList.Count; i++)
				underButtonsList [i].GetComponent<MenuButtonComponent> ().buttonIndex = i;
		}
		else
			content = transform.GetChild (1).GetComponent<RectTransform> ();		

		//Call Setup In Under Menu
		if(underMenuList.Count > 0)
			for (int i = 0; i < underMenuList.Count; i++)
				underMenuList [i].GetComponent<MenuComponent> ().OtherMenuSetup ();

		HideAll ();

		DisableAll ();
	}

	void HideAll ()
	{
		if(menuComponentType == MenuComponentType.MainMenu || menuComponentType == MenuComponentType.ButtonsListMenu)
			for (int i = 0; i < underButtonsList.Count; i++)
				underButtonsList[i].anchoredPosition = new Vector2(MenuManager.Instance.offScreenX, MenuManager.Instance.buttonsYPositions [i]);

		if (button != null)
			button.anchoredPosition = new Vector2 (MenuManager.Instance.offScreenX, button.anchoredPosition.y);

		if (content != null)
			content.anchoredPosition = new Vector2 (MenuManager.Instance.offScreenX, content.anchoredPosition.y);
	}

	void DisableAll ()
	{
		if(menuComponentType == MenuComponentType.MainMenu || menuComponentType == MenuComponentType.ButtonsListMenu)
			for (int i = 0; i < underButtonsList.Count; i++)
				underButtonsList[i].gameObject.SetActive (false);
		
		if (button != null)
			button.gameObject.SetActive (false);

		if (content != null)
			content.gameObject.SetActive (false);
	}

	public void Submit (int buttonIndex)
	{
		if (menuComponentType == MenuComponentType.ButtonsListMenu || menuComponentType == MenuComponentType.MainMenu)
			MenuManager.Instance.ShowUnderButtons (otherButtonsList, buttonIndex, underButtonsList, this);
		
		else
			MenuManager.Instance.ShowContent (otherButtonsList, buttonIndex, content, this);
	}

	public void Cancel ()
	{
		if (menuComponentType == MenuComponentType.ButtonsListMenu)
			MenuManager.Instance.HideUnderButtons (underButtonsList, aboveMenuScript, button);
		
		else if (menuComponentType == MenuComponentType.ContentMenu)
			MenuManager.Instance.HideContent (content, aboveMenuScript, button);
	}
}
