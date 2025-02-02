import { Children } from "react";

export default function Popup({
	open,
	close,
	children,
}: {
	open: boolean;
	close: () => void;
	children?: any;
}) {
	return (
		<div
			className={`fixed top-0 z-50 h-screen w-screen ${open ? "visible" : "invisible"}`}
		>
			<div
				className="fixed -z-50 h-full w-full bg-gray-800 opacity-80"
				onClick={close}
			></div>
			<div
				onClick={close}
				className="absolute right-5 top-5 cursor-pointer rounded-lg bg-red-500 p-1 text-white"
			>
				x
			</div>
			{children}
		</div>
	);
}
