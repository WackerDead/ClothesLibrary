import { useEffect, useState } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import "./App.css";
import Products from "./Clothes/Clothes";
import Navbar from "./Navbar";
import Outfits from "./Outfits/Outfits";
import { ClothesProvider } from "./ClothesContext";

export default function App() {
	const [forecasts, setForecasts] = useState([]);
	const [loading, setLoading] = useState(true);

	const populateWeatherData = async () => {
		const response = await fetch("/api/weatherforecast");
		const data = await response.json();
		setForecasts(data);
		setLoading(false);
	};

	const populateClothesData = async () => {
		const response = await fetch("/api/clothes");
		const data = await response.json();
		console.log(data);
	};

	useEffect(() => {
		//populateWeatherData();
	}, []);

	/*const content = loading ? (
    <p>
      <em>Loading...</em>
    </p>
  ) : (
    renderForecastsTable(forecasts)
  );*/

	return (
		<div className="App">
			<BrowserRouter>
				<ClothesProvider>
					<Routes>
						<Route path="/" element={<Products />} />
						<Route path="/fits" element={<Outfits />} />
					</Routes>
					<Navbar />
				</ClothesProvider>
			</BrowserRouter>
		</div>
	);
}

function renderForecastsTable(forecasts: any) {
	return (
		<table className="table-striped table" aria-labelledby="tabelLabel">
			<thead>
				<tr>
					<th>Date</th>
					<th>Temp. (C)</th>
					<th>Temp. (F)</th>
					<th>Summary</th>
				</tr>
			</thead>
			<tbody>
				{forecasts.map((forecast: any) => (
					<tr key={forecast.date}>
						<td>{forecast.date}</td>
						<td>{forecast.temperatureC}</td>
						<td>{forecast.temperatureF}</td>
						<td>{forecast.summary}</td>
					</tr>
				))}
			</tbody>
		</table>
	);
}
function renderClothes(clothes: any) {
	return (
		<table className="table-striped table" aria-labelledby="tabelLabel">
			<thead>
				<tr>
					<th>Name</th>
					<th>Brand</th>
					<th>Color</th>
					<th>Type</th>
				</tr>
			</thead>
			<tbody>
				{clothes.map((product: any) => (
					<tr key={product.name}>
						<td>{product.name}</td>
						<td>{product.brand}</td>
						<td>{product.color}</td>
						<td>{product.type}</td>
					</tr>
				))}
			</tbody>
		</table>
	);
}
