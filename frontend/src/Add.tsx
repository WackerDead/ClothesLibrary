import { FormEvent, useState } from "react";
import ClothesDetails from "./ClothesDetails";

export default function Add({ close }: { close: () => void }) {
	function AddProduct(e: FormEvent<HTMLFormElement>) {
		e.preventDefault();

		const formData = new FormData(e.currentTarget);
		console.log(formData);

		fetch("/api/clothes", {
			method: "POST",
			body: formData,
		});
	}

	return ClothesDetails({
		product: null,
		submit: AddProduct,
		close: close,
	});
}

export function Edit() {}
