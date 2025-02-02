import { FormEvent, useEffect, useState } from "react";
import Popup from "./Popup";
import { Clothing } from "./Clothes";
import axios from "axios";

export function Add({ open, close }: { open: boolean; close: () => void }) {
	return ClothingPopup({
		clothing: null,
		submit: (e: FormEvent<HTMLFormElement>) => {
			e.preventDefault();

			const formData = new FormData(e.currentTarget);
			console.log(formData);

			/*fetch("/api/clothes", {
				method: "POST",
				body: formData,
			});*/
			axios.post("api/clothes", formData);
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
	return ClothingPopup({
		clothing: clothing,
		submit: async (e: FormEvent<HTMLFormElement>) => {
			e.preventDefault();

			const formData = new FormData(e.currentTarget);

			let response = await fetch(
				"http://localhost:5000/Upload/" + clothing.imageName,
			);
			let blob = await response.blob();
			let file = new File([blob], clothing.imageName);

			formData.set("image", file);

			console.log(formData);
			axios.put(`api/clothes/${clothing.id}`, formData);
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
	clothing = clothing || {
		name: "",
		brand: "",
		type: "",
		imageName: "",
	};

	const [image, setImage] = useState("");
	// const [imageFile, setImageFile] = useState<Promise<File> | null>(async () => {
	// 	let response = await fetch(
	// 		"http://localhost:5000/Upload/" + clothing.imageName,
	// 	);
	// 	let blob = await response.blob();
	// 	return new File([blob], clothing.imageName);
	// });

	useEffect(() => {
		setImage(
			clothing.imageName === ""
				? ""
				: "http://localhost:5000/Upload/" + clothing.imageName,
		);
	}, [clothing]);

	const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		if (e.target.files && e.target.files[0]) {
			setImage(URL.createObjectURL(e.target.files[0]));
			//setImageFile(async () => e.target.files[0]);
		}
	};

	const handleImageClick = () => {
		document.getElementById("image")?.click();
	};

	const hasImage = image ? " " : "border-gray-300 border-2 rounded-md";

	return (
		<Popup open={open} close={close}>
			<div className="absolute left-1/2 top-1/3 flex h-fit w-1/2 -translate-x-1/2 -translate-y-1/3 transform flex-col items-center justify-center rounded-lg border-2 border-gray-300 bg-white px-12 py-20">
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
						close();
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
							defaultValue={clothing.type}
						>
							<option value="tshirt">Tshirt</option>
							<option value="pants">Pants</option>
							<option value="shoes">Shoes</option>
							<option value="sweatshirt">Sweatshirt</option>
							<option value="hoodie">Hoodie</option>
						</select>
					</label>
					<input
						type="file"
						id="image"
						name="image"
						accept="image/png, image/jpg, image/jpeg"
						className="hidden"
						onChange={handleImageChange}
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
	);
}
