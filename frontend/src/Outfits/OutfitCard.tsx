import { useState } from "react";
import { Clothing, uintToHex } from "../Clothes/Clothes";
import chroma from "chroma-js";
import { Outfit } from "./Outfits";

export default function OutfitCard({
	clothing,
	editMethod,
}: {
	clothing: Outfit;
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
				className="aspect-square w-96 object-contain"
				src={"api/Upload/Outfits/" + clothing.imageName}
				alt=""
			/>
			<h1>{clothing.name}</h1>
		</div>
	);
}
