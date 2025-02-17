import { Link } from "react-router";

const Header = () => {
  return (
    <header className="bg-white shadow-md  border-b border-blue-400">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 flex justify-between items-center h-16">
        <Link to="/" className="text-xl font-bold text-gray-900">
          ChatDocs{" "}
          <span className="text-xs font-light text-gray-900">
            – AI Powered Document Search
          </span>
        </Link>
      </div>
    </header>
  );
};

export default Header;
