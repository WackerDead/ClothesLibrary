import { useState } from "react";
import { Clothing } from "./Clothes";

export default function ClothesCard({
	clothing,
	editMethod,
}: {
	clothing: Clothing;
	editMethod: () => void;
}) {
	return (
		<div className="relative m-4 flex w-[400px] flex-col items-center justify-center rounded-lg border-2 border-gray-300 p-4">
			<div
				onClick={editMethod}
				className="absolute right-0 top-0 cursor-pointer rounded-lg bg-red-500 p-1 text-white"
			>
				e
			</div>
			<img
				className="aspect-square w-96 object-cover"
				src={"http://localhost:5000/Upload/" + clothing.imageName}
				alt=""
			/>
			<h1>{clothing.name}</h1>
			<h2>{clothing.brand}</h2>
			<h3>{clothing.type}</h3>
		</div>
	);
}
