import { useEffect, useState } from "react";
import ClothesCard from "./ClothesCard";
import ClothesDetails from "./ClothesDetails";
import axios from "axios";
import { Edit } from "./ClothesRegistration";

export type Clothing = {
	id?: number;
	name: string;
	brand: string;
	type: string;
	imageName: string;
};

export default function Products() {
	const [clothes, setClothes] = useState([] as Clothing[]);
	const [editing, setEditing] = useState(false);
	const [clothToEdit, setClothToEdit] = useState({} as Clothing);

	const populateClothesData = async () => {
		const response = await fetch("/api/clothes");
		const data = await response.json();
		setClothes(data);
	};

	useEffect(() => {
		populateClothesData();
	}, []);

	return (
		<div>
			{/* <div>
				{editing ? (
					<ClothesDetails
						product={clothToEdit}
						submit={(e) => {
							console.log("asdad");
							const formData = new FormData(e.currentTarget);
							let a: Blob;
							fetch("http://localhost:5000/Upload" + clothToEdit.imageName)
								.then((res) => res.blob())
								.then((res) => (a = res));
							console.log(a);
							console.log(formData);
							formData.set("image", a);
							axios.put("api/clothes/" + clothToEdit.id, formData);
						}}
						close={() => {
							setEditing(false);
							setClothToEdit({} as Clothing);
						}}
					/>
				) : (
					""
				)}
			</div> */}
			<Edit
				clothing={clothToEdit}
				open={editing}
				close={() => setEditing(false)}
			/>
			<div className="flex flex-row flex-wrap justify-center">
				{clothes.map((clothing) => (
					<ClothesCard
						clothing={clothing}
						editMethod={() => {
							setEditing(true);
							setClothToEdit(clothing);
						}}
					/>
				))}
			</div>
		</div>
	);
}
