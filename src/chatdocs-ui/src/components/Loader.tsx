import React from "react";
import { RotatingLines } from "react-loader-spinner";

interface LoaderProps {
  className?: string;
}

const Loader: React.FC<LoaderProps> = ({ className }) => {
  return (
    <div className={`loader ${className}`}>
      <RotatingLines
        strokeColor="grey"
        strokeWidth="2"
        animationDuration="0.75"
        width="24"
        visible={true}
      />
    </div>
  );
};

export default Loader;
