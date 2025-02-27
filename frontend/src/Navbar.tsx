import { NavLink } from "react-router-dom";
import { Add } from "./Clothes/ClothesRegistration";
import { useState } from "react";
import { UseClothes } from "./ClothesContext";
import { AddFit } from "./Outfits/OutfitRegistration";

const Navbar = () => {
	const [adding, setAdding] = useState(false);
	const { refreshClothes } = UseClothes();
	return (
		<div>
			{window.location.pathname === "/fits" ? (
				<AddFit open={adding} close={() => setAdding(false)} />
			) : (
				<Add open={adding} close={() => setAdding(false)} />
			)}
			<footer className="h-16">
				<nav className="fixed bottom-0 mx-auto flex h-16 w-full items-center justify-around border-t border-gray-300 bg-white">
					<div
						className="flex h-full w-full cursor-pointer items-center justify-center"
						onClick={async () => {
							const input = document.createElement("input");
							input.type = "file";
							input.accept = ".zip";
							input.click();
							input.onchange = async () => {
								const formData = new FormData();
								formData.append("file", input.files?.[0] as Blob);
								await fetch("/api/clothes/upload", {
									method: "POST",
									body: formData,
								}).then(() => refreshClothes());
							};
						}}
					>
						📤
					</div>
					<NavItem path="/" icon="🏠" />
					<div
						className="flex h-full w-full cursor-pointer items-center justify-center"
						onClick={() => setAdding(true)}
					>
						➕
					</div>
					<NavItem path="/fits" icon="👤" />
					<div
						className="flex h-full w-full cursor-pointer items-center justify-center"
						onClick={async () => {
							const response = await fetch("/api/clothes/download");
							const blob = await response.blob();
							const url = window.URL.createObjectURL(blob);
							const a = document.createElement("a");
							a.href = url;
							a.download = "export.zip";
							document.body.appendChild(a);
							a.click();
							document.body.removeChild(a);
						}}
					>
						📦
					</div>
				</nav>
			</footer>
		</div>
	);
};

const NavItem = ({ path, icon }: { path: string; icon: string }) => {
	return (
		<NavLink
			className="flex h-full w-full items-center justify-center"
			to={path}
		>
			{icon}
		</NavLink>
	);
};

export default Navbar;
