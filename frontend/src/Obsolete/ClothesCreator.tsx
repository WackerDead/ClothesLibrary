import { FormEvent, useEffect, useState } from "react";
import { Clothing } from "../Clothes/Clothes";

export default function ClothesDetails({
	product,
	submit,
	close,
}: {
	product: Clothing;
	submit: (e: FormEvent<HTMLFormElement>) => void;
	close: () => void;
}) {
	product = product || {
		name: "",
		brand: "",
		type: "",
		imageName: "",
	};

	const [image, setImage] = useState(
		product.imageName === ""
			? ""
			: "http://localhost:5000/Upload/" + product.imageName,
	);

	const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
		if (e.target.files && e.target.files[0]) {
			setImage(URL.createObjectURL(e.target.files[0]));
		}
	};

	const handleImageClick = () => {
		document.getElementById("image")?.click();
	};

	const hasImage = image ? " " : "border-gray-300 border-2 rounded-md";

	return (
		<div className="fixed top-0 z-50 h-screen w-screen">
			<div className="fixed -z-50 h-full w-full bg-gray-800 opacity-80"></div>
			<div
				onClick={close}
				className="absolute right-5 top-5 cursor-pointer rounded-lg bg-red-500 p-1 text-white"
			>
				xa
			</div>
			<div className="absolute left-1/2 top-1/3 flex h-fit w-1/2 -translate-x-1/2 -translate-y-1/3 transform flex-col items-center justify-center rounded-lg border-2 border-gray-300 bg-white px-12 py-20">
				<h1>{product.name === "" ? "Add a new item" : "Edit the item"}</h1>
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
							defaultValue={product.name}
						/>
					</label>
					<label>
						Brand
						<input
							type="text"
							id="brand"
							name="brand"
							defaultValue={product.brand}
						/>
					</label>
					<label>
						Type
						<select
							name="type"
							id="type"
							autoComplete="on"
							defaultValue={product.type}
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
		</div>
	);
}
