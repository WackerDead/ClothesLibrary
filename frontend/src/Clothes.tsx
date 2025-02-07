import { useEffect, useState } from "react";
import ClothesCard from "./ClothesCard";
import ClothesDetails from "./ClothesDetails";
import axios from "axios";
import { Edit } from "./ClothesRegistration";
import { UseClothes } from "./ClothesContext";
import ClothesFilter from "./ClothesFilter";

export type Clothing = {
	id?: number;
	name: string;
	brand: string;
	type: string;
	imageName: string;
	colors?: any[];
	isWaiting?: boolean;
};

export type ClothesFilterType = {
	brand?: string[];
	type?: string[];
	colors?: string[];
};

export const uintToHex = (c: number) => {
	const hex = c.toString(16).padStart(6, "0");
	return "#" + hex;
};

export default function Clothes() {
	const { clothes } = UseClothes();
	const [filtered, setFiltered] = useState<Clothing[]>(clothes);
	const [editing, setEditing] = useState(false);
	const [clothToEdit, setClothToEdit] = useState<Clothing | null>(null);
	const [filter, setFilter] = useState<ClothesFilterType>({});

	const onFilter = (f: ClothesFilterType) => {
		console.log(filter);
		const filteredClothes = clothes.filter((c) => {
			if (f.brand && f.brand.length > 0 && !f.brand.includes(c.brand)) {
				return false;
			}
			if (f.type && f.type.length > 0 && !f.type.includes(c.type)) {
				return false;
			}
			if (!c.isWaiting && !c.colors) {
				throw new Error(`Clothing item ${c.name} has no colors`);
			}
			if (
				c.colors != undefined &&
				f.colors &&
				f.colors.length > 0 &&
				!c.colors.map((c) => c).some((color) => f.colors.includes(color.name))
			) {
				return false;
			}
			return true;
		});
		setFiltered(filteredClothes);
	};

	useEffect(() => {
		onFilter(filter);
		console.log(clothes);
	}, [clothes]);

	return (
		<div>
			<ClothesFilter
				onFilter={(f) => {
					setFilter(f);
					onFilter(f);
				}}
			/>
			<Edit
				clothing={clothToEdit}
				open={editing}
				close={() => {
					setEditing(false);
					setClothToEdit(null);
				}}
			/>
			<div className="flex flex-row flex-wrap justify-center">
				{filtered.map((clothing) => (
					<ClothesCard
						key={clothing.id}
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
