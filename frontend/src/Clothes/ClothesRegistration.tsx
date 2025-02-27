import { FormEvent, useEffect, useRef, useState } from "react";
import Popup from "../Popup";
import { Clothing } from "./Clothes";
import axios from "axios";
import { UseClothes } from "../ClothesContext";

export function Add({ open, close }: { open: boolean; close: () => void }) {
	const { addClothing } = UseClothes();
	return ClothingPopup({
		clothing: null,
		submit: async (e: FormEvent<HTMLFormElement>) => {
			e.preventDefault();

			const formData = new FormData(e.currentTarget);
			const imagePath = e.currentTarget.image.value;
			const imageName = imagePath.split("\\").pop(); // Extract the file name

			console.log(formData);
			const newClothing: Clothing = {
				name: formData.get("name") as string,
				brand: formData.get("brand") as string,
				type: formData.get("type") as string,
				imageName: imageName || "", // Will be updated after upload
				isWaiting: true,
			};
			console.log(newClothing);
			addClothing(newClothing);

			/*fetch("/api/clothes", {
				method: "POST",
				body: formData,
			});*/
			try {
				console.log(formData);
				const response = await axios.post("api/clothes", formData);
				addClothing(response.data);
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

export function Edit({
	clothing,
	open,
	close,
}: {
	clothing: Clothing;
	open: boolean;
	close: () => void;
}) {
	const { editClothing } = UseClothes();
	return ClothingPopup({
		clothing: clothing,
		submit: async (e: FormEvent<HTMLFormElement>) => {
			e.preventDefault();

			const imagePath = e.currentTarget.image.value;
			let imageName: string = imagePath.split("\\").pop(); // Extract the file name
			const formData = new FormData(e.currentTarget);
			if (e.currentTarget.image.value == "") {
				let response = await fetch(
					"http://localhost:5000/Upload/" + clothing.imageName,
				);
				let blob = await response.blob();
				let file = new File([blob], clothing.imageName);

				formData.set("image", file);
				imageName = clothing.imageName;
			}

			const updatedClothing: Clothing = {
				id: clothing.id,
				name: formData.get("name") as string,
				brand: formData.get("brand") as string,
				type: formData.get("type") as string,
				imageName: imageName || "", // Will be updated after upload
				isWaiting: true,
			};
			editClothing(updatedClothing);

			try {
				const response = await axios.put(
					`api/clothes/${clothing.id}`,
					formData,
				);
				editClothing(response.data);
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

export function ClothingPopup({
	clothing,
	submit,
	open,
	close,
}: {
	clothing: Clothing;
	submit: (e: FormEvent<HTMLFormElement>) => void;
	open: boolean;
	close: () => void;
}) {
	const exists = clothing !== null;
	clothing = clothing || {
		name: "",
		brand: "",
		type: "",
		imageName: "",
	};

	const [image, setImage] = useState("");
	const imageInputRef = useRef<HTMLInputElement>(null);
	const { deleteClothing } = UseClothes();

	useEffect(() => {
		setImage(
			clothing.imageName === ""
				? ""
				: "http://localhost:5000/Upload/" + clothing.imageName,
		);
	}, [open]);

	const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		if (e.target.files && e.target.files[0]) {
			setImage(URL.createObjectURL(e.target.files[0]));
		}
	};

	const handleImageClick = () => {
		imageInputRef.current.click();
	};

	const hasImage = image ? " " : "border-gray-300 border-2 rounded-md";

	return (
		open && (
			<Popup open={open} close={close}>
				<div className="absolute left-1/2 top-1/3 flex h-fit w-1/2 -translate-x-1/2 -translate-y-1/3 transform flex-col items-center justify-center rounded-lg border-2 border-gray-300 bg-white px-12 py-20">
					<div
						onClick={() => {
							axios.delete(`api/clothes/${clothing.id}`).then(() => {
								deleteClothing(clothing);
								return close();
							});
						}}
						className="absolute right-5 top-5 cursor-pointer rounded-lg bg-red-500 p-1 text-white"
					>
						d
					</div>
					<h1>{clothing.name === "" ? "Add a new item" : "Edit the item"}</h1>
					<div
						className={
							"flex aspect-square w-96 cursor-pointer items-center justify-center object-contain" +
							hasImage
						}
						onClick={handleImageClick}
					>
						{image ? (
							<img src={image} className="h-full w-full object-contain" />
						) : (
							<span>Click to select an image</span>
						)}
					</div>
					<form
						className="addProdForm mx-auto flex w-fit flex-col items-start p-4"
						onSubmit={(e) => {
							e.preventDefault();
							submit(e);
							//TODO: when added or edited, "refresh" the pages
						}}
					>
						<label>
							Name
							<input
								type="text"
								id="name"
								name="name"
								defaultValue={clothing.name}
							/>
						</label>
						<label>
							Brand
							<input
								type="text"
								id="brand"
								name="brand"
								defaultValue={clothing.brand}
							/>
						</label>
						<label>
							Type
							<select
								name="type"
								id="type"
								autoComplete="on"
								defaultValue={
									ClothingTypes.find((t) => t.value === clothing.type)?.value
								}
							>
								{ClothingTypes.map((t) => {
									return <option value={t.value}>{t.label}</option>;
								})}
							</select>
						</label>
						<input
							type="file"
							id="image"
							name="image"
							accept="image/png, image/jpg, image/jpeg"
							className="hidden"
							onChange={handleImageChange}
							ref={imageInputRef}
						/>
						<button
							className="mt-2 rounded-md border-2 border-gray-300 p-1 px-2"
							type="submit"
						>
							Submit
						</button>
					</form>
				</div>
			</Popup>
		)
	);
}

export const ClothingTypes = [
	{ value: "Tshirt", label: "Tshirt" },
	{ value: "Pants", label: "Pants" },
	{ value: "Shoes", label: "Shoes" },
	{ value: "Sweatshirt", label: "Sweatshirt" },
	{ value: "Hoodie", label: "Hoodie" },
];
