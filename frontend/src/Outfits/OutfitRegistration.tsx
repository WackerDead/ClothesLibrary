import { FormEvent, useEffect, useRef, useState } from "react";
import { Outfit } from "./Outfits";
import Popup from "../Popup";
import axios from "axios";
import { ClothingTypes } from "../Clothes/ClothesRegistration";
import { UseClothes } from "../ClothesContext";
import Select from "react-select";

export function AddFit({ open, close }: { open: boolean; close: () => void }) {
	return OutfitPopup({
		outfit: null,
		submit: async (clothes: number[]) => {
			console.log("submit");

			try {
				const response = await axios.post("api/outfits", clothes);
			} catch (error) {
				console.error(error);
			} finally {
				close();
			}
		},
		open: open,
		close: close,
	});
}
export function EditFit({
	outfit,
	open,
	close,
}: {
	outfit: Outfit;
	open: boolean;
	close: () => void;
}) {
	return OutfitPopup({
		outfit: outfit,
		submit: async (clothes: number[]) => {
			console.log("submit");
		},
		open: open,
		close: close,
	});
}

export function OutfitPopup({
	outfit,
	submit,
	open,
	close,
}: {
	outfit: Outfit;
	submit: (clothes: number[]) => void;
	open: boolean;
	close: () => void;
}) {
	const exists = outfit !== null;
	outfit = outfit || {
		name: "",
		clothes: [],
		imageName: "",
	};

	const [selectedClothes, setSelectedClothes] = useState<
		{
			name: string;
			id: number;
		}[]
	>([]);
	const { clothes } = UseClothes();

	const [image, setImage] = useState("");
	const imageInputRef = useRef<HTMLInputElement>(null);
	//const { deleteClothing } = UseClothes();

	useEffect(() => {
		setImage(
			outfit.imageName === ""
				? ""
				: "http://localhost:5000/Upload/Outfits/" + outfit.imageName,
		);
	}, [open]);

	const handleChange = (selectedOptions, actionMeta) => {
		console.log(selectedOptions);
		let opt = selectedOptions.value;
		const { name } = actionMeta;
		setSelectedClothes((prevFilter) => [
			...prevFilter,
			{ name: name, id: opt },
		]);
	};

	const onSubmit = () => {
		submit(selectedClothes.map((c) => c.id));
	};

	const getDefaultValue = (type: string) => {
		const c = outfit.clothes.filter((c) => c.type == type)[0];
		return c != null ? { label: c.name, value: c.id } : null;
	};

	return (
		open && (
			<Popup open={open} close={close}>
				<div className="absolute left-1/2 top-1/3 flex h-fit w-1/2 -translate-x-1/2 -translate-y-1/3 transform flex-col items-center justify-center rounded-lg border-2 border-gray-300 bg-white px-12 py-20">
					<div
						onClick={() => {
							axios.delete(`api/outfits/${outfit.id}`).then(() => {
								//deleteClothing(clothing);
								return close();
							});
						}}
						className="absolute right-5 top-5 cursor-pointer rounded-lg bg-red-500 p-1 text-white"
					>
						d
					</div>
					{exists && (
						<div
							className={
								"my-2 flex aspect-square w-96 cursor-pointer items-center justify-center object-contain"
							}
						>
							<img src={image} className="h-full w-full object-contain" />
						</div>
					)}
					<Select
						className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
						name="hoodie"
						options={
							clothes
								.filter((c) => c.type == "Hoodie")
								.map((c) => ({
									label: c.name,
									value: c.id,
								})) as any
						}
						onChange={handleChange}
						defaultValue={getDefaultValue("Hoodie")}
						/*styles={styles}*/
					/>
					<Select
						className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
						name="pants"
						options={
							clothes
								.filter((c) => c.type == "Pants")
								.map((c) => ({
									label: c.name,
									value: c.id,
								})) as any
						}
						onChange={handleChange}
						defaultValue={getDefaultValue("Pants")}
						/*styles={styles}*/
					/>
					<Select
						className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
						name="shoes"
						options={
							clothes
								.filter((c) => c.type == "Shoes")
								.map((c) => ({
									label: c.name,
									value: c.id,
								})) as any
						}
						onChange={handleChange}
						defaultValue={getDefaultValue("Shoes")}
						/*styles={styles}*/
					/>
					<button
						className="mt-2 rounded-md border-2 border-gray-300 p-1 px-2"
						type="submit"
						onClick={onSubmit}
					>
						Submit
					</button>
				</div>
			</Popup>
		)
	);
}
