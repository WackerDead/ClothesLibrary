import { createContext, useContext, useEffect, useState } from "react";
import { Clothing } from "./Clothes/Clothes";

type ClothesContextType = {
	clothes: Clothing[];
	addClothing: (clothing: Clothing) => void;
	editClothing: (clothing: Clothing) => void;
	deleteClothing: (clothing: Clothing) => void;
	refreshClothes: () => void;
};

const ClothesContext = createContext<ClothesContextType | undefined>(undefined);

export function ClothesProvider({ children }: { children: React.ReactNode }) {
	const [clothes, setClothes] = useState<Clothing[]>([]);

	const addClothing = (clothing: Clothing) => {
		// First time
		if (clothing.isWaiting == true) {
			setClothes((prev) => [...prev, clothing]);
		} else {
			// Fill in the missing data
			setClothes((prev) =>
				prev.map((c) => (c.isWaiting == true ? clothing : c)),
			);
		}
	};
	const editClothing = (clothing: Clothing) => {
		setClothes((prev) => {
			const index = prev.findIndex((c) => c.id === clothing.id);
			prev[index] = clothing;
			return [...prev];
		});
	};

	const deleteClothing = (clothing: Clothing) => {
		setClothes((prev) => prev.filter((c) => c.id !== clothing.id));
	};

	const refreshClothes = async () => {
		const response = await fetch("/api/clothes");
		const data = await response.json();
		setClothes(data);
	};

	useEffect(() => {
		refreshClothes();
	}, []);

	return (
		<ClothesContext.Provider
			value={{
				clothes,
				addClothing: addClothing,
				editClothing: editClothing,
				deleteClothing: deleteClothing,
				refreshClothes,
			}}
		>
			{children}
		</ClothesContext.Provider>
	);
}

export function UseClothes() {
	const context = useContext(ClothesContext);
	if (!context) {
		throw new Error("useClothes must be used within a ClothesProvider");
	}
	return context;
}
