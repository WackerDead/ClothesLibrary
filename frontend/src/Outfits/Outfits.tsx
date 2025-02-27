import { useEffect, useState } from "react";
import { UseClothes } from "../ClothesContext";
import OutfitCard from "./OutfitCard";
import Select from "react-select";
import { ClothingTypes } from "../Clothes/ClothesRegistration";
import axios, { formToJSON } from "axios";
import { EditFit } from "./OutfitRegistration";
import { Clothing } from "../Clothes/Clothes";

export type Outfit = {
	id?: number;
	name: string;
	clothes: Clothing[];
	imageName: string;
};

export default function Outfits() {
	const [outfits, setOutfits] = useState<Outfit[]>([]);
	const { clothes } = UseClothes();
	const [selectedClothes, setSelectedClothes] = useState<
		{
			name: string;
			id: number;
		}[]
	>([]);
	const [editing, setEditing] = useState(false);
	const [outfitToEdit, setOutfitToEdit] = useState<Outfit | null>(null);

	const populateOutfitsData = () => {
		axios.get("api/outfits").then((response) => setOutfits(response.data));
		/*setOutfits([
			{
				id: 1,
				name: "Outfit 1",
				clothes: [24, 25, 26],
				imageName: "outfit1.jpg",
			},
			{
				id: 2,
				name: "Outfit 2",
				clothes: [26, 33, 41],
				imageName: "outfit2.jpg",
			},
		]);*/
	};

	useEffect(() => {
		populateOutfitsData();
	}, []);

	const handleChange = (selectedOptions, actionMeta) => {
		console.log(selectedOptions);
		let opt = selectedOptions.value;
		const { name } = actionMeta;
		setSelectedClothes((prevFilter) => [
			...prevFilter,
			{ name: name, id: opt },
		]);
	};

	return (
		<div>
			<div>
				<Select
					className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
					name="hoodie"
					options={clothes
						.filter((c) => c.type == "Hoodie")
						.map((c) => ({
							label: c.name,
							value: c.id,
						}))}
					onChange={handleChange}
					/*styles={styles}*/
				/>
				<Select
					className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
					name="pants"
					options={clothes
						.filter((c) => c.type == "Pants")
						.map((c) => ({
							label: c.name,
							value: c.id,
						}))}
					onChange={handleChange}
					/*styles={styles}*/
				/>
				<Select
					className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
					name="shoes"
					options={clothes
						.filter((c) => c.type == "Shoes")
						.map((c) => ({
							label: c.name,
							value: c.id,
						}))}
					onChange={handleChange}
					/*styles={styles}*/
				/>
				<button
					onClick={() => {
						let formData = new FormData();
						console.log(selectedClothes);
						const s = selectedClothes.map((c) => c.id);
						console.log(s);
						formData.append("name", "Outfit 3");
						selectedClothes.forEach((c) => {
							formData.append("clothes", c.id.toString());
						});
						//formData.append("clothes", JSON.stringify(s));
						console.log(formData);

						axios.post("api/outfits", formData).then((response) => {
							console.log(response);
						});
					}}
				>
					asd
				</button>
			</div>
			<EditFit
				outfit={outfitToEdit}
				open={editing}
				close={() => {
					setEditing(false);
					setOutfitToEdit(null);
				}}
			/>
			{outfits.map((outfit) => (
				<OutfitCard
					clothing={outfit}
					editMethod={() => {
						setEditing(true);
						setOutfitToEdit(outfit);
					}}
				/>
			))}
		</div>
	);
}
