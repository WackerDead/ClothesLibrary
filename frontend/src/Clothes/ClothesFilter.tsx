import axios from "axios";
import { ClothingTypes } from "./ClothesRegistration";
import { FormEvent, FormEventHandler, useEffect, useState } from "react";
import { ClothesFilterType, uintToHex } from "./Clothes";
import { UseClothes } from "../ClothesContext";
import Select, { Options, StylesConfig } from "react-select";
import chroma from "chroma-js";

export default function ClothesFilter({
	onFilter,
}: {
	onFilter: (filter: ClothesFilterType) => void;
}) {
	const { clothes } = UseClothes();
	const types = ClothingTypes;
	const [brands, setBrands] = useState<string[]>([]);
	const [colors, setColors] = useState([]);
	const [filter, setFilter] = useState<ClothesFilterType>({
		type: [],
		brand: [],
		colors: [],
	});

	const populateFilters = () => {
		axios
			.get("api/clothes/brands")
			.then((response) => setBrands(response.data));
		axios.get("api/colors").then((response) => setColors(response.data));
	};

	const onChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
		const formData = new FormData(e.currentTarget.form);
		const jsonData = {
			type: [...formData.getAll("type")],
			brand: [...formData.getAll("brand")],
			color: [...formData.getAll("colors")],
		};
		console.log(jsonData);

		const filter = {
			type: jsonData.type as string[],
			brand: jsonData.brand as string[],
			colors: jsonData.color as string[],
		};
		onFilter(filter);
	};

	const handleChange = (selectedOptions, actionMeta) => {
		console.log(selectedOptions);
		let opt = selectedOptions.map((option) => option.value);
		const { name } = actionMeta;
		setFilter((prevFilter) => ({
			...prevFilter,
			[name]: opt,
		}));
		onFilter({
			...filter,
			[name]: opt,
		});
	};

	const styles: StylesConfig = {
		control: (styles) => ({ ...styles, background: "white" }),
		option: (styles, { data, isSelected, isFocused }) => ({
			...styles,
			backgroundColor: isFocused ? "lightgrey" : "white",
		}),
		input: (styles) => ({ ...styles }),
		placeholder: (styles) => ({ ...styles }),
		singleValue: (styles, { data }) => ({ ...styles }),
	};
	const dot = (color = "transparent") => ({
		alignItems: "center",
		display: "flex",

		":before": {
			backgroundColor: color,
			borderRadius: 10,
			border: `1px solid ${chroma(color).darken(1).css()}`,
			content: '" "',
			display: "block",
			marginRight: 4,
			height: 16,
			width: 16,
		},
	});

	const colourStyles: StylesConfig<any, true> = {
		control: (styles) => ({ ...styles, backgroundColor: "white" }),
		option: (styles, { data, isDisabled, isFocused, isSelected }) => {
			const color = chroma(data.color);
			return {
				...styles,
				...dot(data.color),
				backgroundColor: isDisabled
					? undefined
					: isSelected
						? data.color
						: isFocused
							? color.alpha(0.1).css()
							: undefined,
				color: isDisabled
					? "#ccc"
					: isSelected
						? chroma.contrast(color, "white") > 2
							? "white"
							: "black"
						: "black",
				cursor: isDisabled ? "not-allowed" : "default",

				":active": {
					...styles[":active"],
					backgroundColor: !isDisabled
						? isSelected
							? data.color
							: color.alpha(0.3).css()
						: undefined,
				},
			};
		},
		multiValue: (styles, { data }) => {
			const color = chroma(data.color);
			return {
				...styles,
				backgroundColor: color.alpha(0.1).css(),
			};
		},
		multiValueLabel: (styles, { data }) => ({
			...styles,
			color:
				chroma.contrast(chroma(data.color), "white") > 1
					? data.color
					: chroma(data.color).darken(3).css(),
		}),
		multiValueRemove: (styles, { data }) => ({
			...styles,
			color: data.color,
			":hover": {
				backgroundColor: data.color,
				color: "white",
			},
		}),
	};

	useEffect(() => {
		populateFilters();
	}, [clothes]);

	return (
		<div>
			<form>
				<div className="mt-6 flex w-full flex-col justify-center sm:flex-row">
					<Select
						className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
						name="type"
						options={types}
						isMulti
						onChange={handleChange}
						styles={styles}
					/>
					<Select
						className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
						name="brand"
						options={brands}
						isMulti
						onChange={handleChange}
						styles={styles}
					/>
					<Select
						className="mx-3.5 my-1.5 sm:my-0 sm:w-1/2 md:w-1/4"
						name="colors"
						options={colors}
						isMulti
						onChange={handleChange}
						styles={colourStyles}
					/>
				</div>

				{/* <select name="type" multiple onChange={onChange}>
					{types.map((type) => (
						<option key={type.value} value={type}>
							{type}
						</option>
					))}
				</select>
				<select name="brand" multiple onChange={onChange}>
					{brands.map((brand) => (
						<option key={brand} value={brand}>
							{brand}
						</option>
					))}
				</select>
				<select name="colors" multiple onChange={onChange}>
					{colors.map((color) => (
						<option
							key={color.id}
							value={color.name}
							style={{ backgroundColor: uintToHex(color.value) }}
						>
							{color.name}
						</option>
					))}
				</select> */}
			</form>
		</div>
	);
}
