﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class HexGrid : MonoBehaviour {

	public int chunkCountX = 4, chunkCountZ = 3;
	public HexCell cellPrefab;
	public Canvas MoveCanvas1;
	public Canvas AttackCanvas1;
	public Canvas DefenceCanvas1;
	public Canvas CreateUnitCanvas1;
	public Canvas DeleteUnitCanvas1;
	public Canvas HexRangeCanvas1;
	public Texture2D noiseSource;
	public Camera camera;
	public Text cellLabelPrefab;
	public HexGridChunk chunkPrefab;
	HexGridChunk[] chunks;
	HexCell[] cells;
	public Color[] colors;
	int cellCountX, cellCountZ;

	void Awake () {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.colors = colors;
		cellCountX = chunkCountX * HexMetrics.chunkSizeX;
		cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

		CreateChunks();
		CreateCells();
    SetRangeOnEachCell();
	}

	void CreateChunks () {
		chunks = new HexGridChunk[chunkCountX * chunkCountZ];

		for (int z = 0, i = 0; z < chunkCountZ; z++) {
			for (int x = 0; x < chunkCountX; x++) {
				HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
				chunk.transform.SetParent(transform);
			}
		}
	}

	void CreateCells () {
		cells = new HexCell[cellCountZ * cellCountX];

		for (int z = 0, i = 0; z < cellCountZ; z++) {
			for (int x = 0; x < cellCountX; x++) {
				CreateCell(x, z, i++);
			}
		}
	}

	void OnEnable () {
		HexMetrics.noiseSource = noiseSource;
		HexMetrics.colors = colors;
	}

	public HexCell GetCell (Vector3 position) {
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index =
			coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
		return cells[index];


	}

	public HexCell GetCell (HexCoordinates coordinates) {
		int z = coordinates.Z;
		if (z < 0 || z >= cellCountZ) {
			return null;
		}
		int x = coordinates.X + z / 2;
		if (x < 0 || x >= cellCountX) {
			return null;
		}
		return cells[x + z * cellCountX];
	}

	public void ShowUI (bool visible) {
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].ShowUI(visible);
		}
	}

	void CreateCell (int x, int z, int i) {
		Vector3 position;
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		Canvas temp = Instantiate<Canvas>(MoveCanvas1);
		Canvas temp2 = Instantiate<Canvas>(AttackCanvas1);
		Canvas temp3 = Instantiate<Canvas>(DefenceCanvas1);
		Canvas temp4 = Instantiate<Canvas>(CreateUnitCanvas1);
		Canvas temp5 = Instantiate<Canvas>(DeleteUnitCanvas1);
		Canvas temp6 = Instantiate<Canvas>(HexRangeCanvas1);
		temp.transform.SetParent(cell.transform, false);
		temp2.transform.SetParent(cell.transform, false);
		temp3.transform.SetParent(cell.transform, false);
		temp4.transform.SetParent(cell.transform, false);
		temp5.transform.SetParent(cell.transform, false);
		temp6.transform.SetParent(cell.transform, false);
		
		cell.MoveCanvas = temp;
		cell.AttackCanvas = temp2;
		cell.DefenceCanvas = temp3;
		cell.CreateCanvas = temp4;
		cell.DeleteCanvas = temp5;
		cell.ActualPosition = cell.transform.position;
		cell.HexRangeCanvas = temp6;
		
		cell.MoveCanvas.enabled = false;
		cell.AttackCanvas.enabled = false;
		cell.DefenceCanvas.enabled = false;
		cell.CreateCanvas.enabled = false;
		cell.DeleteCanvas.enabled = false;

		if (x > 0) {
			cell.SetNeighbor(HexDirection.W, cells[i - 1]);
		}
		if (z > 0) {
			if ((z & 1) == 0) {
				cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
				if (x > 0) {
					cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
				}
			}
			else {
				cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
				if (x < cellCountX - 1) {
					cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
				}
			}
		}
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.anchoredPosition =
			new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;

		cell.Elevation = 0;

		AddCellToChunk(x, z, cell);

	}

	void AddCellToChunk (int x, int z, HexCell cell) {
		int chunkX = x / HexMetrics.chunkSizeX;
		int chunkZ = z / HexMetrics.chunkSizeZ;
		HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

		int localX = x - chunkX * HexMetrics.chunkSizeX;
		int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
		chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
	}
	public void OccupyCell(HexCell cell){
		int index = cell.coordinates.X + cell.coordinates.Z * cellCountX + cell.coordinates.Z / 2;
		cells[index].isOccupied = true; 
	}
	public void UnOccupyCell(HexCell cell){
		int index = cell.coordinates.X + cell.coordinates.Z * cellCountX + cell.coordinates.Z / 2;
		cells[index].isOccupied = false;

	}

	public HexCell MovementCell(Vector3 position){
		HexCell currCell = GetCell(position);
		currCell.MoveCanvas.transform.LookAt(camera.transform.position);
		currCell.MoveCanvas.enabled = true;
		return currCell;
	}


	public HexCell CreateUnitCell(Vector3 position){
		HexCell currCell = GetCell(position);
		currCell.CreateCanvas.transform.LookAt(camera.transform.position);
		if(currCell.isOccupied == false){
			currCell.CreateCanvas.enabled = true;
		}
		return currCell;
	}

	public HexCell AttackCell(Vector3 position){
		HexCell currCell = GetCell(position) ;
		currCell.AttackCanvas.transform.LookAt(camera.transform.position);
		currCell.AttackCanvas.enabled = true;
		return currCell;
	}



	public HexCell DefenceCell(Vector3 position){
		HexCell currCell = GetCell( position);
		currCell.DefenceCanvas.transform.LookAt(camera.transform.position);
		currCell.DefenceCanvas.enabled = true;
		return currCell;
	}

	public HexCell DeleteCell(Vector3 position){
		HexCell currCell = GetCell( position);
		currCell.DeleteCanvas.transform.LookAt(camera.transform.position);
		currCell.DeleteCanvas.enabled = true;
		return currCell;

		}


	/*public HexCell getCell(Vector3 position){
		//the same as "touchCell" but different name 
		position = transform.InverseTransformPoint(position);
		HexCoordinates coordinates = HexCoordinates.FromPosition(position);
		int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
		HexCell cell = cells[index];
		cell.color = touchedColor;
		hexMesh.Triangulate(cells);
		
		return cell; 
		
	}*/
//make button face camera 
	public void MoveButton(Canvas ButtonCanvas){
		ButtonCanvas.transform.LookAt(camera.transform.position);
	}

	

	//button calls this to disable the button in the game
    public void DisableButton(Canvas canvas){
        canvas.enabled = false;
    }
	
	public void Save (BinaryWriter writer) {
		for (int i = 0; i < cells.Length; i++) {
			cells[i].Save(writer);
		}
	}

	public void Load (BinaryReader reader) {
		for (int i = 0; i < cells.Length; i++) {
			cells[i].Load(reader);
		}
		for (int i = 0; i < chunks.Length; i++) {
			chunks[i].Refresh();
		}
	}

	public void PlacementBlueTeam(Vector3 position){
		HexCell cell = GetCell(position);
		cell.BlueCanPlace = true; 
	}
	public void PlacementRedTeam(Vector3 position){
		HexCell cell = GetCell(position);
		cell.RedCanPlace = true; 
	}

	public void SetRangeOnEachCell(){
		foreach (HexCell cell in cells)
		{
				cell.SetRange();
		}
	}
}