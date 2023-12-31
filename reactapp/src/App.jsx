import React, { useState } from "react";
import { useEffect } from "react";
import axios from "axios";
import MainPage from "./pages/MainPage/Index.jsx";
import CurrentAssets from "./pages/MyAccount/CurrentAssets/Index.jsx";
import TransactionHistory from "./pages/MyAccount/TransactionHistory/Index.jsx";
import { Route, Routes } from "react-router-dom";

const API_SECOND_LIMIT = 60;

const App = () => {
	const [coins, setCoins] = useState([{}, {}]);
	const [isReady, setReady] = useState(false);
	//Function to get Coins
	const makeRequest = async () => {
		await axios
			.get("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=100&page=1&sparkline=false&price_change_percentage=1h%2C24h%2C7d&locale=en")
			.then((res) => {
				setCoins(res.data);
				localStorage.setItem("coins", JSON.stringify(res.data));
				localStorage.setItem("lastRequestTime", Date.now());
			})
			.catch((err) => console.log(err));
	};

	const isReadyFunction = async () => {
		console.log("checking if ready");
		await axios
			.get("/api/isReady") //
			.then((res) => setReady(res.data))
			.catch((err) => console.log(err));
	};

	useEffect(() => {
		// API Rate Limit Logic
		const lastRequestTime = localStorage.getItem("lastRequestTime");
		const currentTime = Date.now();

		if (!lastRequestTime || currentTime - lastRequestTime > API_SECOND_LIMIT * 1000) {
			makeRequest();
			console.log("getting coins");
		} else {
			setCoins(JSON.parse(localStorage.getItem("coins")));
			console.log("api limit exceeded, retrieving coins from localstorage");
		}

		isReadyFunction();
		const intervalId = setInterval(isReadyFunction, 2000);
		if (isReady) {
			clearInterval(intervalId);
		}
		return () => clearInterval(intervalId);
	}, [isReady]);

	if (isReady) {
		return (
			<Routes>
				<Route
					path="/"
					element={<MainPage coins={coins} />}
				/>
				<Route
					path="/myAccount/assets"
					element={<CurrentAssets coins={coins} />}
				/>
				<Route
					path="/myAccount/transactionHistory"
					element={<TransactionHistory />}
				/>
			</Routes>
		);
	} else {
		return <h1>Loading the server...</h1>;
	}
};

export default App;
