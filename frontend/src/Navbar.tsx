import { NavLink } from "react-router-dom";
import Add from "./Add";
import { useState } from "react";

const Navbar = () => {
	const [adding, setAdding] = useState(false);
	return (
		<footer>
			{adding && <Add close={() => setAdding(false)} />}
			<nav className="fixed bottom-0 mx-auto flex h-16 w-full items-center justify-around border-t border-gray-300 bg-white">
				<NavItem path="/" icon="🏠" />
				<div
					className="flex h-full w-full cursor-pointer items-center justify-center"
					onClick={() => setAdding(true)}
				>
					➕
				</div>
				<NavItem path="/fits" icon="👤" />
			</nav>
		</footer>
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
